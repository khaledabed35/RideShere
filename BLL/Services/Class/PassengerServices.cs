using BLL.DTOs;
using BLL.Helper;
using BLL.Services.Interface;
using DAL.DTOs;
using DAL.DTOs.Auth;
using DAL.DTOs.DriverDTO;
using DAL.Models;
using DAL.Reposetoriy;
using DAL.Specification;
using DAL.Specification.Class;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using StackExchange.Redis;

namespace BLL.Services.Class
{
    public class PassengerServices : IPassengerService

    {
        private readonly IGenaricRepo<App_User> _User;
        private readonly IGenaricRepo<Trip> _trip;
        private readonly IGenaricRepo<Driver> _driver;
        private readonly IGenaricRepo<Review> _review;
        private readonly IGenaricRepo<Offer> _offerRepo;

        private readonly StackExchange.Redis.IDatabase _redisDb;
        private readonly IHubContext<RideHub> _hubContext;

        public PassengerServices(
            IGenaricRepo<App_User> user,
            IGenaricRepo<Trip> trip,
            IGenaricRepo<Review> review,
            IGenaricRepo<Driver> driver,
            IConnectionMultiplexer redisConnection,
            IHubContext<RideHub> hubContext,
            IGenaricRepo<Offer> offerRepo



            )
        {
            _User = user;
            _trip = trip;
            _review = review;
            _driver = driver;
            _redisDb = redisConnection.GetDatabase();
            _hubContext = hubContext;
            _offerRepo= offerRepo;
        }
        public async Task AcceptDriverOfferAsync(Guid passengerId, Guid offerId)
        {
            var offerSpec = new OfferSpecification(offerId);
            var offer = await _offerRepo.GetByIdWithSpecAsync(offerSpec);

            if (offer == null || offer.Trip.PassengerId != passengerId)
                throw new Exception("Offer not found or unauthorized.");

            var trip = offer.Trip;

            trip.Status = TripStatus.Accepted;
            trip.DriverId = offer.DriverId;
            trip.AgreedFare = offer.OfferedPrice;

            _trip.Update(trip);

            await _hubContext.Clients.Group(offer.DriverId.ToString())
                .SendAsync("RideAcceptedByPassenger", new { TripId = trip.Id });

            var tripOffersSpec = new OfferSpecification(trip.Id);
            var tripOffers = await _offerRepo.GetAllWithSpecAsync(tripOffersSpec);

            // 5. رفض العروض الأخرى وإبلاغ أصحابها
            foreach (var otherOffer in tripOffers)
            {
                if (otherOffer.Id != offerId)
                {
                    otherOffer.Status = OfferStatus.Rejected;
                    _offerRepo.Update(otherOffer);

                    await _hubContext.Clients.Group(otherOffer.DriverId.ToString())
                        .SendAsync("RideNoLongerAvailable", new { TripId = trip.Id });
                }
            }

            await _trip.SaveChangesAsync();
        }

        public async Task AddTripReviewAsync(Guid passengerId, Guid tripId, string? reason)
        {
            // 1. التأكد أن الرحلة موجودة وتخص الراكب نفسه
            var trip = await _trip.GetByAsync(t => t.Id == tripId && t.PassengerId == passengerId);

            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found or unauthorized.");
            }

            // 2. التحقق من أن الرحلة مكتملة قبل السماح بتقديم التقييم أو التعليق
            if (trip.Status != TripStatus.Completed)
            {
                throw new InvalidOperationException("You can only review completed trips.");
            }

            // 3. التحقق من عدم وجود تقييم سابق لنفس الرحلة لمنع التكرار
            var existingReview = await _review.GetByAsync(r => r.TripId == tripId);
            if (existingReview != null)
            {
                throw new InvalidOperationException("A review for this trip has already been submitted.");
            }

            // 4. إنشاء سجل التقييم الجديد وربطه بالرحلة والسائق
            var review = new Review
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                DriverId = trip.DriverId ,
                Comment = reason, // حفظ السبب أو التعليق المُرسل
                CreatedAt = DateTime.UtcNow
            };

            // 5. حفظ البيانات في قاعدة البيانات
            await _review.AddAsync(review);
            await _review.SaveChangesAsync();
        }
        // }

        public async Task CancelTripAsync(Guid passengerId, Guid tripId, string reason)
        {
            var trip = await _trip.GetByAsync(t => t.Id == tripId && t.PassengerId == passengerId);

            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found or unauthorized.");
            }
            if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
            {
                throw new InvalidOperationException("This trip cannot be cancelled.");
            }
            trip.Status = TripStatus.Cancelled;

            trip.TripStatusLogs.Add(new TripStatusLog
            {
                Id = Guid.NewGuid(),
                TripId = trip.Id,
                Status = TripStatus.Cancelled,
                Notes = reason,
                createdAt = DateTime.UtcNow
            });

            _trip.Update(trip);
            await _trip.SaveChangesAsync();
            if (trip.DriverId != Guid.Empty)
            {
                await _hubContext.Clients.Group(trip.DriverId.ToString()).SendAsync("TripCancelledByPassenger", new
                {
                    TripId = trip.Id,
                    Reason = reason
                });
            }
        }
      


          public async Task<RideStatusDto?> GetCurrentTripAsync(Guid passengerId)
        {
            var spec = new TripSpecification(passengerId);
            var activeTrip = await _trip.GetByIdWithSpecAsync(spec);

            // التحقق من null أولاً
            if (activeTrip == null)
                return null;

            // الآن نتحقق من حالة الرحلة المنتهية صلاحيتها
            if (activeTrip.Status == TripStatus.Requested && activeTrip.CreateAt.AddMinutes(30) <= DateTime.UtcNow)
            {
                activeTrip.Status = TripStatus.Cancelled;
                _trip.Update(activeTrip);
                await _trip.SaveChangesAsync();
                return null;
            }

            // باقي الكود...
            double driverLatitude = 0;
            double driverLongitude = 0;
            // ...
        

            if (activeTrip.DriverId != null)
            {
                var geo = await _redisDb.GeoPositionAsync("drivers_locations", activeTrip.DriverId.ToString());
                if (geo.HasValue)
                {
                    driverLatitude = geo.Value.Latitude;
                    driverLongitude = geo.Value.Longitude;
                }
            }

            return new RideStatusDto
            {
                RideId = activeTrip.Id,
                Status = activeTrip.Status.ToString(),
                CurrentLatitude = driverLatitude,
                CurrentLongitude = driverLongitude,
                DriverName = activeTrip.Driver?.Name,
                DriverPhone = activeTrip.Driver?.PhoneNumber,
                DriverImage = activeTrip.Driver?.ImageUrl,
                carModel = activeTrip.Car?.Model,


            };
        }
        public async Task<IEnumerable<AvailableDriverDto>> GetNearbyDriversAsync(decimal latitude, decimal longitude)
        {
            double ridenearby = 5;

            // 1. جلب السائقين الأقرب من Redis
            var nearbyGeoResults = await _redisDb.GeoRadiusAsync(
                "drivers_locations",
                (double)longitude,
                (double)latitude,
                ridenearby,
                GeoUnit.Kilometers,
                count: 10,
                Order.Ascending
            );

            // 2. استخراج الـ IDs الخاصة بالسائقين وتحويلها لـ List
            var driverIds = nearbyGeoResults
                .Select(x => Guid.TryParse(x.Member.ToString(), out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            if (!driverIds.Any())
                return Enumerable.Empty<AvailableDriverDto>();

            var driversSpec = new DriverSpecification(driverIds);
            var driversFromDb = await _driver.GetAllWithSpecAsync(driversSpec);

            var availableDrivers = nearbyGeoResults
                .Select(geoItem =>
                {
                    if (!Guid.TryParse(geoItem.Member.ToString(), out Guid driverId))
                        return null;

                    var driver = driversFromDb.FirstOrDefault(d => d.Id == driverId);

                    // التحقق أن السائق موجود وأنه Online بالفعل
                    if (driver == null || driver.Status != DriverStatus.Online)
                        return null;

                    return new AvailableDriverDto
                    {
                        DriverId = driver.Id,
                        Name = driver.Name,
                        PhoneNumber = driver.PhoneNumber,
                        Rating = driver.Rating,
                        CurrentLatitude = geoItem.Position?.Latitude ?? 0,
                        CurrentLongitude = geoItem.Position?.Longitude ?? 0,
                        DistanceInKm = geoItem.Distance ?? 0
                    };
                })
                .Where(dto => dto != null)
                .Cast<AvailableDriverDto>()
                .ToList();

            return availableDrivers;
        }




        public async Task<IEnumerable<DriverOfferDto>> GetTripOffersAsync(Guid passengerId, Guid tripId)
        {
            var trip = await _trip.GetByAsync(t => t.Id == tripId && t.PassengerId == passengerId);
            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found or unauthorized.");
            }

            var spec = new OfferSpecification(tripId); 
            var offers = await _offerRepo.GetAllWithSpecAsync(spec);

            var offerDtos = offers.Select(o => new DriverOfferDto
            {
                OfferId = o.Id,
                DriverId = o.DriverId,
                DriverName = o.Driver.User.FullName, 
                DriverImage = o.Driver.User.ImageUrl ?? string.Empty,
                Rating = o.Driver.Rating,

                CarModel = o.Driver.DriverDocument != null ? o.Driver.DriverDocument.CarModel : string.Empty,
                CarCategory = o.Driver.DriverDocument != null ? o.Driver.DriverDocument.CarCategory : string.Empty,

                ProposedFare = o.OfferedPrice,

            
            }).ToList();

            return offerDtos;
        }


        public async Task<RideDetailsDto> RequestRideAsync(Guid passengerId, RequestRideDto model)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var pickupPoint = geometryFactory.CreatePoint(new Coordinate((double)model.PickupLongitude, (double)model.PickupLatitude));
            var dropoffPoint = geometryFactory.CreatePoint(new Coordinate((double)model.DestinationLongitude, (double)model.DestinationLatitude));

            var trip = new Trip
            {
                Id = Guid.NewGuid(),
                PassengerId = passengerId,
                Status = TripStatus.Requested,
                RequestedAt = DateTimeOffset.UtcNow,
                CreateAt = DateTime.UtcNow,
                PickupLocation = pickupPoint,
                DropoffLocation = dropoffPoint,
                EstimatedFare = 0, 
                AgreedFare = 0
            };
            await _trip.AddAsync(trip);
            await _trip.SaveChangesAsync();

            var nearbyDrivers = await GetNearbyDriversAsync(model.PickupLatitude, model.PickupLongitude);

            foreach (var driver in nearbyDrivers)
            {
                await _hubContext.Clients.Group(driver.DriverId.ToString()).SendAsync("ReceiveNewRideRequest", new
                {
                    TripId = trip.Id,
                    model.PickupLatitude,
                    model.DestinationLongitude,
                    model.CarType,
                    ProposedFare = model.PassengerProposedFare, 
                    DistanceInKm = driver.DistanceInKm
                });
            }

            return new RideDetailsDto
            {
                RideId = trip.Id,
                Status = trip.Status.ToString(),

                PickupLocation = $"{model.PickupLatitude}, {model.PickupLongitude}",
                Destination = $"{model.DestinationLatitude}, {model.DestinationLongitude}",
                // ...
                FinalFare = 0,
                PaymentMethod = trip.PaymentMethod.ToString(),
                CreatedAt = trip.CreateAt,
                Offers = new List<DriverOfferDto>(),
                message = "Ride request created with your proposed fare, waiting for drivers...",
            };
        }
    }
}



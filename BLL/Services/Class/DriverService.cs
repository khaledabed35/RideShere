using BLL.Helper;
using BLL.Services.Interface;
using DAL.DTOs;
using DAL.DTOs.DriverDTO;
using DAL.Models;
using DAL.Reposetoriy;
using DAL.Specification;
using DAL.Specification.Class;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Class
{
    public class DriverService : IDriverService
    {

        private readonly IGenaricRepo<App_User> _User;
        private readonly IGenaricRepo<Trip> _trip;
        private readonly IGenaricRepo<Driver> _driver;
        private readonly IGenaricRepo<Review> _review;
        private readonly IGenaricRepo<Offer> _offerRepo;

        private readonly StackExchange.Redis.IDatabase _redisDb;
        private readonly IHubContext<RideHub> _hubContext;
        public DriverService(IGenaricRepo<App_User> user,
            IGenaricRepo<Trip> trip,
            IGenaricRepo<Review> review,
            IGenaricRepo<Driver> driver,
            IConnectionMultiplexer redisConnection,
            IHubContext<RideHub> hubContext,
            IGenaricRepo<Offer> offerRepo)
        {
            _driver= driver;
            _hubContext= hubContext;
            _offerRepo= offerRepo;
            _redisDb=redisConnection.GetDatabase();
            _review= review;
            _trip= trip;
            _User= user;
        }
        public async Task AcceptRideAsync(Guid driverId, Guid rideId)
        {
            var activeTrip = await _trip.GetByAsync(t =>
                t.DriverId == driverId &&
                (t.Status == TripStatus.Accepted ||
                 t.Status == TripStatus.Arrived ||
                 t.Status == TripStatus.InProgress));

            if (activeTrip != null)
            {
                throw new InvalidOperationException("Driver already has an active ride and cannot accept a new one.");
            }

            var trip = await _trip.GetByAsync(t => t.Id == rideId);
            if (trip == null)
            {
                throw new KeyNotFoundException("Ride not found.");
            }

            if (trip.Status != TripStatus.Requested)
            {
                throw new InvalidOperationException("This ride is no longer available.");
            }

            var driver = await _driver.GetByAsync(d => d.Id == driverId);
            if (driver == null || driver.Status != DriverStatus.Online)
            {
                throw new InvalidOperationException("Driver is offline or unavailable.");
            }

            // 5. تحديث حالة الرحلة وربطها بالسائق
            trip.Status = TripStatus.Accepted;
            trip.DriverId = driverId;

            // 6. إضافة سجل لتغير الحالة (TripStatusLog)
            if (trip.TripStatusLogs == null)
            {
                trip.TripStatusLogs = new List<TripStatusLog>();
            }

            trip.TripStatusLogs.Add(new TripStatusLog
            {
                Id = Guid.NewGuid(),
                TripId = trip.Id,
                Status = TripStatus.Accepted,
                Notes = "Ride accepted by driver",
                createdAt = DateTime.UtcNow
            });

            _trip.Update(trip);

            try
            {
                await _trip.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("This ride was already accepted by another driver.");
            }

            // 7. إشعار الراكب عبر SignalR
            await _hubContext.Clients.Group(trip.PassengerId.ToString())
                .SendAsync("RideAcceptedByDriver", new
                {
                    TripId = trip.Id,
                    DriverId = driverId
                });
        }
        public async Task<IEnumerable<TripHistoryDto>> GetDriverRideHistoryAsync(Guid driverId, string filterPeriod = "all")
        {
            var spec = new DriverHistorySpecification(driverId, filterPeriod);
            var trips = await _trip.GetAllWithSpecAsync(spec);

            if (trips == null || !trips.Any())
                return Enumerable.Empty<TripHistoryDto>();

            var historyList = new List<TripHistoryDto>();

            foreach (var trip in trips)
            {
                double pickupLat = trip.PickupLocation != null ? trip.PickupLocation.Coordinate.Y : 0;
                double pickupLon = trip.PickupLocation != null ? trip.PickupLocation.Coordinate.X : 0;
                double dropoffLat = trip.DropoffLocation != null ? trip.DropoffLocation.Coordinate.Y : 0;
                double dropoffLon = trip.DropoffLocation != null ? trip.DropoffLocation.Coordinate.X : 0;

                string passengerName = "Passenger";
                if (trip.PassengerId != Guid.Empty)
                {
                    var passenger = await _User.GetByAsync(u => u.Id == trip.PassengerId);
                    if (passenger != null) passengerName = passenger.FullName;
                }

                historyList.Add(new TripHistoryDto
                {
                    TripId = trip.Id,
                    PassengerName = passengerName,
                    Status = trip.Status.ToString(),
                    PickupLocation = $"{pickupLat}, {pickupLon}",
                    DestinationLocation = $"{dropoffLat}, {dropoffLon}",
                    Fare = trip.AgreedFare > 0 ? trip.AgreedFare : trip.EstimatedFare,
                    CreatedAt = trip.CreateAt
                });
            }

            return historyList;
        }
        public async Task CancelRideByDriverAsync(Guid driverId, Guid rideId, string reason)
        {
            var trip = await _trip.GetByAsync(t => t.Id == rideId && t.DriverId == driverId);
            if (trip == null)
            {
                throw new KeyNotFoundException("Ride not found or you are not authorized to cancel this ride.");
            }

            if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
            {
                throw new InvalidOperationException("This trip cannot be cancelled.");
            }

            trip.Status = TripStatus.Cancelled;

            if (trip.TripStatusLogs != null)
            {
                trip.TripStatusLogs.Add(new TripStatusLog
                {
                    Id = Guid.NewGuid(),
                    TripId = trip.Id,
                    Status = TripStatus.Cancelled,
                    Notes = reason,
                    createdAt = DateTime.UtcNow
                });
            }

            _trip.Update(trip);

            var driver = await _driver.GetByAsync(d => d.Id == driverId);
            if (driver != null)
            {
                driver.Status = DriverStatus.Online;
                _driver.Update(driver);
            }

            await _trip.SaveChangesAsync();

            await _hubContext.Clients.Group(trip.PassengerId.ToString())
                .SendAsync("TripCancelledByDriver", new
                {
                    TripId = trip.Id,
                    Reason = reason
                });
        }

        public async Task<IEnumerable<RideRequestDto>> GetAvailableRideRequestsAsync(decimal driverLatitude, decimal driverLongitude)
        {
            double maxDistanceKm = 10;
            var thresholdTime = DateTime.UtcNow.AddMinutes(-30);

            var spec = new TripSpecification(thresholdTime);
            var requestedTrips = await _trip.GetAllWithSpecAsync(spec);

            if (requestedTrips == null || !requestedTrips.Any())
            {
                return Enumerable.Empty<RideRequestDto>();
            }

            var availableRides = new List<RideRequestDto>();

            foreach (var trip in requestedTrips)
            {
                if (trip.PickupLocation == null) continue;

                double tripLat = trip.PickupLocation.Coordinate.Y;
                double tripLon = trip.PickupLocation.Coordinate.X;

                // حساب المسافة
                double distanceInKm = CalculateDistance((double)driverLatitude, (double)driverLongitude, tripLat, tripLon);

                if (distanceInKm <= maxDistanceKm)
                {
                    string passengerName = trip.Passenger != null ? trip.Passenger.FullName : "Passenger";

                    availableRides.Add(new RideRequestDto
                    {
                        RideId = trip.Id,
                        passengerid = trip.PassengerId,
                        PassengerName = passengerName,
                        PickupLocation = $"{tripLat}, {tripLon}",
                        Destination = trip.DropoffLocation != null ? $"{trip.DropoffLocation.Coordinate.Y}, {trip.DropoffLocation.Coordinate.X}" : string.Empty,
                        PickupLatitude = (decimal)tripLat,
                        PickupLongitude = (decimal)tripLon,
                        DestinationLatitude = trip.DropoffLocation != null ? (decimal)trip.DropoffLocation.Coordinate.Y : 0,
                        DestinationLongitude = trip.DropoffLocation != null ? (decimal)trip.DropoffLocation.Coordinate.X : 0,
                        EstimatedFare = trip.EstimatedFare,
                        ProposedFare = trip.EstimatedFare,
                        DistanceInKm = Math.Round(distanceInKm, 2),
                        CreatedAt = trip.CreateAt
                    });
                }
            }

            return availableRides.OrderBy(r => r.DistanceInKm);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371 * c;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        public async Task<RideDetailsDto?> GetCurrentRideAsync(Guid driverId)
        {
            // 1. جلب كل الرحلات ثم البحث عن الرحلة النشطة الحالية الخاصة بالسائق
            var allTrips = await _trip.GetAllAsync();

            if (allTrips == null || !allTrips.Any())
                return null;

            var activeTrip = allTrips.FirstOrDefault(t =>
                t.DriverId == driverId &&
                (t.Status == TripStatus.Accepted ||
                 t.Status == TripStatus.Arrived ||
                 t.Status == TripStatus.InProgress));

            if (activeTrip == null)
                return null;

            double pickupLat = activeTrip.PickupLocation != null ? activeTrip.PickupLocation.Coordinate.Y : 0;
            double pickupLon = activeTrip.PickupLocation != null ? activeTrip.PickupLocation.Coordinate.X : 0;
            double dropoffLat = activeTrip.DropoffLocation != null ? activeTrip.DropoffLocation.Coordinate.Y : 0;
            double dropoffLon = activeTrip.DropoffLocation != null ? activeTrip.DropoffLocation.Coordinate.X : 0;

            return new RideDetailsDto
            {
                RideId = activeTrip.Id,
                Status = activeTrip.Status.ToString(),
                PickupLocation = $"{pickupLat}, {pickupLon}",
                Destination = $"{dropoffLat}, {dropoffLon}",
                FinalFare = activeTrip.AgreedFare > 0 ? activeTrip.AgreedFare : activeTrip.EstimatedFare,
                PaymentMethod = activeTrip.PaymentMethod.ToString(),
                CreatedAt = activeTrip.RequestedAt != default ? activeTrip.RequestedAt : activeTrip.CreateAt,
                CompletedAt = null,
                DriverId = driverId,
                message = "Current active ride retrieved successfully.",
                Offers = new List<DriverOfferDto>()
            };
        }

        public async Task<DriverEarningsDto> GetEarningsAsync(Guid driverId)
        {
            // 1. جلب كل الرحلات من الـ Repository
            var allTrips = await _trip.GetAllAsync();

            if (allTrips == null || !allTrips.Any())
            {
                return new DriverEarningsDto
                {
                    TotalEarnings = 0,
                    CompletedRidesCount = 0,
                    TodayEarnings = 0,
                    ThisWeekEarnings = 0
                };
            }

            var completedTrips = allTrips
                .Where(t => t.DriverId == driverId && t.Status == TripStatus.Completed)
                .ToList();

            if (!completedTrips.Any())
            {
                return new DriverEarningsDto
                {
                    TotalEarnings = 0,
                    CompletedRidesCount = 0,
                    TodayEarnings = 0,
                    ThisWeekEarnings = 0
                };
            }

            decimal totalEarnings = completedTrips.Sum(t => t.AgreedFare > 0 ? t.AgreedFare : t.EstimatedFare);
            int totalTrips = completedTrips.Count;

            var today = DateTime.UtcNow.Date;
            decimal todayEarnings = completedTrips
                .Where(t => t.CreateAt.Date == today)
                .Sum(t => t.AgreedFare > 0 ? t.AgreedFare : t.EstimatedFare);

            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            decimal weeklyEarnings = completedTrips
                .Where(t => t.CreateAt.Date >= weekStart)
                .Sum(t => t.AgreedFare > 0 ? t.AgreedFare : t.EstimatedFare);

            return new DriverEarningsDto
            {
                TotalEarnings = totalEarnings,
                CompletedRidesCount = totalTrips,
                TodayEarnings = todayEarnings,
                ThisWeekEarnings = weeklyEarnings
            };
        }
        public async Task ProposeNewFareAsync(Guid driverId, Guid rideId, decimal newFare)
        {
            // 1. التأكد أن الرحلة موجودة
            var trip = await _trip.GetByAsync(t => t.Id == rideId);
            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found.");
            }

            if (trip.Status != TripStatus.Requested)
            {
                throw new InvalidOperationException("This trip is no longer accepting offers.");
            }

            var driver = await _driver.GetByAsync(d => d.Id == driverId);
            if (driver == null || driver.Status != DriverStatus.Online)
            {
                throw new InvalidOperationException("Driver not found or is currently offline.");
            }

            var existingOffer = await _offerRepo.GetByAsync(o => o.TripId == rideId && o.DriverId == driverId);

            if (existingOffer != null)
            {
                existingOffer.OfferedPrice = newFare;
                existingOffer.Status = OfferStatus.Pending;
                existingOffer.CreatedAt = DateTime.UtcNow;
                _offerRepo.Update(existingOffer);
            }
            else
            {
                var offer = new Offer
                {
                    Id = Guid.NewGuid(),
                    TripId = rideId,
                    DriverId = driverId,
                    OfferedPrice = newFare,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _offerRepo.AddAsync(offer);
            }

            await _offerRepo.SaveChangesAsync();

            await _hubContext.Clients.Group(trip.PassengerId.ToString())
                .SendAsync("ReceiveDriverOffer", new
                {
                    TripId = trip.Id,
                    DriverId = driverId,
                    DriverName = driver.Name,
                    OfferedPrice = newFare
                });
        }

        public async Task RejectRideAsync(Guid driverId, Guid rideId)
        {
            var trip = await _trip.GetByAsync(t => t.Id == rideId);
            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found.");
            }

            var driver = await _driver.GetByAsync(d => d.Id == driverId);
            if (driver == null)
            {
                throw new KeyNotFoundException("Driver not found.");
            }

            var existingOffer = await _offerRepo.GetByAsync(o => o.TripId == rideId && o.DriverId == driverId);
            if (existingOffer != null)
            {
                existingOffer.Status = OfferStatus.Rejected;
                _offerRepo.Update(existingOffer);
                await _offerRepo.SaveChangesAsync();
            }

           
        }
        public async Task UpdateDriverAvailabilityAsync(Guid driverId, bool isOnline)
        {
            var driver = await _driver.GetByAsync(d => d.Id == driverId);
            if (driver == null)
            {
                throw new KeyNotFoundException("Driver not found.");
            }

            driver.Status = isOnline ? DriverStatus.Online : DriverStatus.Offline;
            _driver.Update(driver);
            await _driver.SaveChangesAsync();

            if (!isOnline)
            {
                await _redisDb.GeoRemoveAsync("drivers_locations", driverId.ToString());
            }
        }

        public async Task UpdateDriverLocationAsync(Guid driverId, decimal latitude, decimal longitude)
        {
            await _redisDb.GeoAddAsync(
                "drivers_locations",
                (double)longitude,
                (double)latitude,
                driverId.ToString()
            );
        }

        public async Task UpdateRideStatusByDriverAsync(Guid driverId, Guid rideId, string status)
        {
            var trip = await _trip.GetByAsync(t => t.Id == rideId && t.DriverId == driverId);
            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found or unauthorized.");
            }

            // تحويل النص الوارد إلى TripStatus Enum
            if (!Enum.TryParse<TripStatus>(status, true, out var newStatus))
            {
                throw new ArgumentException("Invalid ride status provided.");
            }

            trip.Status = newStatus;

            // توثيق الحالة في سجلات الرحلة
            if (trip.TripStatusLogs == null)
            {
                trip.TripStatusLogs = new List<TripStatusLog>();
            }

            trip.TripStatusLogs.Add(new TripStatusLog
            {
                Id = Guid.NewGuid(),
                TripId = trip.Id,
                Status = newStatus,
                Notes = $"Status updated to {newStatus} by driver",
                createdAt = DateTime.UtcNow
            });

            _trip.Update(trip);

            if (newStatus == TripStatus.Completed)
            {
                var driver = await _driver.GetByAsync(d => d.Id == driverId);
                if (driver != null)
                {
                    driver.Status = DriverStatus.Online;
                    _driver.Update(driver);
                }
            }

            await _trip.SaveChangesAsync();

            await _hubContext.Clients.Group(trip.PassengerId.ToString())
                .SendAsync("RideStatusChanged", new
                {
                    TripId = trip.Id,
                    Status = newStatus.ToString()
                });
        }
    }
}

using BLL.Error;
using BLL.Helper;
using BLL.Services.Interface;
using DAL.DTOs;
using DAL.Models;
using DAL.Reposetoriy;
using DAL.Specification.Class;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace BLL.Services.Class
{
    public class RideService : IRideService
    {
        private readonly IGenaricRepo<App_User> _userRepo;
        private readonly IGenaricRepo<Trip> _tripRepo;
        private readonly IGenaricRepo<Driver> _driverRepo;
        private readonly IGenaricRepo<Review> _reviewRepo;
        private readonly IGenaricRepo<Offer> _offerRepo;

        private readonly IDatabase _redisDb;
        private readonly IHubContext<RideHub> _hubContext;

        public RideService(
            IGenaricRepo<App_User> userRepo,
            IGenaricRepo<Trip> tripRepo,
            IGenaricRepo<Review> reviewRepo,
            IGenaricRepo<Driver> driverRepo,
            IConnectionMultiplexer redisConnection,
            IHubContext<RideHub> hubContext,
            IGenaricRepo<Offer> offerRepo)
        {
            _userRepo = userRepo;
            _tripRepo = tripRepo;
            _reviewRepo = reviewRepo;
            _driverRepo = driverRepo;
            _redisDb = redisConnection.GetDatabase();
            _hubContext = hubContext;
            _offerRepo = offerRepo;
        }

        public async Task<RideDetailsDto?> GetActiveTripAsync(Guid userId)
        {
            var spec = new TripSpecification(userId);
            var trip = await _tripRepo.GetByIdWithSpecAsync(spec);

            if (trip == null)
                return null;

            return MapToRideDetailsDto(trip);
        }

        public async Task<RideDetailsDto> GetTripDetailsAsync(Guid tripId, Guid userId)
        {
            var spec = new TripSpecification(tripId, userId);
            var trip = await _tripRepo.GetByIdWithSpecAsync(spec);

            if (trip == null)
                throw new NotFoundException("Trip not found or you do not have permission to view it.");

            return MapToRideDetailsDto(trip);
        }

        public async Task<IEnumerable<TripHistoryDto>> GetTripHistoryAsync(Guid userId)
        {
            var spec = new TripSpecification(userId);
            var trips = (await _tripRepo.GetAllWithSpecAsync(spec))?.ToList();

            if (trips == null || trips.Count == 0)
                return Enumerable.Empty<TripHistoryDto>();

            return trips.Select(t => new TripHistoryDto
            {
                TripId = t.Id,
                Status = t.Status.ToString(),
                CreatedAt = t.CreateAt,
                Fare = t.AgreedFare > 0 ? t.AgreedFare : t.EstimatedFare,
                PickupLocation = t.PickupLocation != null ? $"{t.PickupLocation.Coordinate.Y}, {t.PickupLocation.Coordinate.X}" : string.Empty,
                DestinationLocation = t.DropoffLocation != null ? $"{t.DropoffLocation.Coordinate.Y}, {t.DropoffLocation.Coordinate.X}" : string.Empty
            }).ToList();
        }

        public async Task<IEnumerable<TripStatusLogDto>> GetTripStatusHistoryAsync(Guid tripId, Guid userId)
        {
            var spec = new TripSpecification(tripId, userId);
            var trip = await _tripRepo.GetByIdWithSpecAsync(spec);

            if (trip == null)
                throw new NotFoundException("Trip not found or you do not have permission to view its status history.");

            if (trip.TripStatusLogs == null || !trip.TripStatusLogs.Any())
                return Enumerable.Empty<TripStatusLogDto>();

            return trip.TripStatusLogs.Select(log => new TripStatusLogDto
            {
                Id = log.Id,
                TripId = log.TripId,
                Status = log.Status,
                CreatedAt = log.createdAt,
                Note = log.Notes
            }).ToList();
        }

        #region Private Helper Methods

        private static RideDetailsDto MapToRideDetailsDto(Trip trip)
        {
            return new RideDetailsDto
            {
                RideId = trip.Id,
                Status = trip.Status.ToString(),
                PickupLocation = trip.PickupLocation != null
                    ? $"{trip.PickupLocation.Coordinate.Y}, {trip.PickupLocation.Coordinate.X}"
                    : string.Empty,
                Destination = trip.DropoffLocation != null
                    ? $"{trip.DropoffLocation.Coordinate.Y}, {trip.DropoffLocation.Coordinate.X}"
                    : string.Empty,
                FinalFare = trip.AgreedFare > 0 ? trip.AgreedFare : trip.EstimatedFare,
                PaymentMethod = trip.PaymentMethod.ToString(),
                CreatedAt = trip.CreateAt,
                Offers = trip.Offers?.Select(o => new DriverOfferDto
                {
                    OfferId = o.Id,
                    DriverId = o.DriverId,
                    ProposedFare = o.OfferedPrice
                }).ToList() ?? new List<DriverOfferDto>()
            };
        }

        #endregion
    }
}
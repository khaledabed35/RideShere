using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.DTOs;
using DAL.DTOs.DriverDTO;

namespace BLL.Services.Interface
{
    public interface IDriverService
    {
        Task<IEnumerable<RideRequestDto>> GetAvailableRideRequestsAsync(decimal driverLatitude, decimal driverLongitude);
        Task AcceptRideAsync(Guid driverId, Guid rideId);
        Task RejectRideAsync(Guid driverId, Guid rideId);
        Task ProposeNewFareAsync(Guid driverId, Guid rideId, decimal newFare);
        Task CancelRideByDriverAsync(Guid driverId, Guid rideId, string reason);
        Task UpdateDriverLocationAsync(Guid driverId, decimal latitude, decimal longitude);
        Task UpdateRideStatusByDriverAsync(Guid driverId, Guid rideId, string status);
        Task UpdateDriverAvailabilityAsync(Guid driverId, bool isOnline);
        Task<RideDetailsDto?> GetCurrentRideAsync(Guid driverId);
        Task<DriverEarningsDto> GetEarningsAsync(Guid driverId);
        Task<IEnumerable<TripHistoryDto>> GetDriverRideHistoryAsync(Guid driverId, string filterPeriod = "all");
        

        }

}
using BLL.DTOs;
using DAL.DTOs;
using DAL.DTOs.Auth;
using DAL.DTOs.DriverDTO;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IPassengerService
    {
        Task<RideDetailsDto> RequestRideAsync( Guid passengerId,RequestRideDto model);

      
        Task<IEnumerable<AvailableDriverDto>> GetNearbyDriversAsync(decimal latitude,decimal longitude);

        Task<IEnumerable<DriverOfferDto>> GetTripOffersAsync(Guid passengerId,Guid tripId);

        Task AcceptDriverOfferAsync(Guid passengerId,Guid offerId);

        Task<RideStatusDto?> GetCurrentTripAsync( Guid passengerId);

        Task CancelTripAsync(Guid passengerId,Guid tripId,string reason);

      
        // Review
        Task AddTripReviewAsync(Guid passengerId, Guid tripId, string reason);
    }
}
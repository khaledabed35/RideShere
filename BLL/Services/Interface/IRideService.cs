using DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface IRideService
    {
        Task<RideDetailsDto> GetTripDetailsAsync(Guid tripId, Guid userId);
        Task<RideDetailsDto?> GetActiveTripAsync(Guid userId);
        Task<IEnumerable<TripHistoryDto>> GetTripHistoryAsync(Guid userId);
        Task<IEnumerable<TripStatusLogDto>> GetTripStatusHistoryAsync(Guid tripId, Guid userId);
    }
}

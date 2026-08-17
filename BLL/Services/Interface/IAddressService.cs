using DAL.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IAddressService
    {
        Task<IEnumerable<FavoritePlaceDto>> GetFavoritePlacesAsync(Guid userId, string? searchTerm = null);
        Task<string> AddFavoritePlaceAsync(Guid userId, FavoritePlaceDto model);
        Task<FavoritePlaceDto> GetFavoritePlaceByIdAsync(Guid userId, int id);
        Task<FavoritePlaceDto> UpdateFavoritePlaceAsync(Guid userId, int id, FavoritePlaceDto model);
        Task DeleteFavoritePlaceAsync(Guid userId, int id);
    }
}
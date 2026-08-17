using BLL.Services.Interface;
using DAL.DTOs.Auth;
using DAL.Models;
using DAL.Reposetoriy;
using DAL.Specification.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class AddressService : IAddressService
    {
        private readonly IGenaricRepo<AddressModel> _address;
        private readonly IGenaricRepo<App_User> _user;

        public AddressService(IGenaricRepo<AddressModel> address, IGenaricRepo<App_User> user)
        {
            _address = address;
            _user = user;
        }

        public async Task DeleteFavoritePlaceAsync(Guid userId, int id)
        {
            var address = await _address.GetByIdAsync(id);
            if (address == null)
            {
                throw new Exception("Address not found.");
            }
            if (address.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized.");

            _address.Delete(address);
            await _address.SaveChangesAsync();
        }

        public async Task<FavoritePlaceDto> GetFavoritePlaceByIdAsync(Guid userId, int id)
        {
            var address = await _address.GetByIdAsync(id);
            if (address == null)
            {
                throw new Exception("Address not found.");
            }
            if (address.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized.");

            return new FavoritePlaceDto
            {
                AddressName = address.AddressName,
                City = address.City,
                Address = address.Address,
            };
        }

        public async Task<FavoritePlaceDto> UpdateFavoritePlaceAsync(Guid userId, int id, FavoritePlaceDto model)
        {
            var address = await _address.GetByIdAsync(id);
            if (address == null)
            {
                throw new Exception("Address not found.");
            }
            if (address.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized.");

            address.AddressName = model.AddressName;
            address.City = model.City;
            address.Address = model.Address;

            _address.Update(address);
            await _address.SaveChangesAsync();

            return new FavoritePlaceDto
            {
                AddressName = address.AddressName,
                City = address.City,
                Address = address.Address,
            };
        }

        public async Task<string> AddFavoritePlaceAsync(Guid userId, FavoritePlaceDto model)
        {
            var address = new AddressModel
            {
                AddressName = model.AddressName,
                City = model.City,
                Address = model.Address,
                UserId = userId
            };

            await _address.AddAsync(address);
            await _address.SaveChangesAsync();
            return "Favorite place added successfully.";
        }

        public async Task<IEnumerable<FavoritePlaceDto>> GetFavoritePlacesAsync(Guid userId, string? searchTerm = null)
        {
            var spec = new AddressSpecification(userId, searchTerm);
            var addresses = await _address.GetAllWithSpecAsync(spec);

            return addresses.Select(a => new FavoritePlaceDto
            {
                AddressName = a.AddressName,
                City = a.City,
                Address = a.Address
            });
        }
    }
}
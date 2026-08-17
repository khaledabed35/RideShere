using DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface IVehicleService
    {
        Task<VehicleDto?> GetMyVehicleAsync(Guid driverId);

        Task<VehicleDto> AddVehicleAsync(
            Guid driverId,
            AddVehicleDto model);

        Task<VehicleDto> UpdateVehicleAsync(
            Guid driverId,
            UpdateVehicleDto model);

        Task DeleteVehicleAsync(Guid driverId);
    }
}

using BLL.Services.Interface;
using DAL.DTOs;
using DAL.Models;
using DAL.Reposetoriy;
using DAL.UnitOfWork.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VehicleDto> AddVehicleAsync(Guid driverId, AddVehicleDto model)
        {
            var driverRepo = _unitOfWork.GetRepository<Driver>();
            var carRepo = _unitOfWork.GetRepository<Car>();

            var driver = await driverRepo.GetByIdAsync(driverId);
            if (driver == null)
                throw new KeyNotFoundException("Driver not found.");

            var existingCar = await carRepo.GetByAsync(c => c.DriverId == driverId);
            if (existingCar != null)
                throw new InvalidOperationException("Driver already has a registered vehicle.");

            var existingPlate = await carRepo.GetByAsync(c => c.PlateNumber == model.PlateNumber);
            if (existingPlate != null)
                throw new InvalidOperationException("Plate number is already registered.");

            var car = new Car
            {
                Id = Guid.NewGuid(),
                DriverId = driverId,
                Brand = model.Brand,
                Model = model.Model,
                Color = model.Color,
                PlateNumber = model.PlateNumber,
                Category = model.Category,
                CarImageUrl = model.CarImageUrl
            };

            await carRepo.AddAsync(car);
            await _unitOfWork.CompleteAsync(); 

            return new VehicleDto
            {
                Id = car.Id,
                DriverId = car.DriverId,
                Brand = car.Brand,
                Model = car.Model,
                Color = car.Color,
                PlateNumber = car.PlateNumber,
                Category = car.Category,
                CarImageUrl = car.CarImageUrl
            };
        }

        public async Task<VehicleDto?> GetMyVehicleAsync(Guid driverId)
        {
            var carRepo = _unitOfWork.GetRepository<Car>();
            var car = await carRepo.GetByAsync(c => c.DriverId == driverId);

            if (car == null)
                return null;

            return new VehicleDto
            {
                Id = car.Id,
                DriverId = car.DriverId,
                Brand = car.Brand,
                Model = car.Model,
                Color = car.Color,
                PlateNumber = car.PlateNumber,
                Category = car.Category,
                CarImageUrl = car.CarImageUrl
            };
        }

        public async Task<VehicleDto> UpdateVehicleAsync(Guid driverId, UpdateVehicleDto model)
        {
            var carRepo = _unitOfWork.GetRepository<Car>();
            var car = await carRepo.GetByAsync(c => c.DriverId == driverId);

            if (car == null)
                throw new KeyNotFoundException("Vehicle not found for this driver.");

            if (car.PlateNumber != model.PlateNumber)
            {
                var existingPlate = await carRepo.GetByAsync(c => c.PlateNumber == model.PlateNumber);
                if (existingPlate != null)
                    throw new InvalidOperationException("Plate number is already registered by another vehicle.");
            }

            car.Brand = model.Brand;
            car.Model = model.Model;
            car.Color = model.Color;
            car.PlateNumber = model.PlateNumber;
            car.Category = model.Category;
            car.CarImageUrl = model.CarImageUrl;

            carRepo.Update(car);
            await _unitOfWork.CompleteAsync();

            return new VehicleDto
            {
                Id = car.Id,
                DriverId = car.DriverId,
                Brand = car.Brand,
                Model = car.Model,
                Color = car.Color,
                PlateNumber = car.PlateNumber,
                Category = car.Category,
                CarImageUrl = car.CarImageUrl
            };
        }

        public async Task DeleteVehicleAsync(Guid driverId)
        {
            var carRepo = _unitOfWork.GetRepository<Car>();
            var car = await carRepo.GetByAsync(c => c.DriverId == driverId);

            if (car == null)
                throw new KeyNotFoundException("Vehicle not found for this driver.");

            carRepo.Delete(car);
            await _unitOfWork.CompleteAsync();
        }
    }
}
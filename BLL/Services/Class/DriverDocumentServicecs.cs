using BLL.DTOs;
using BLL.Services.Interface;
using DAL.Models;
using DAL.Reposetoriy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class DriverDocumentService : IDriverDocumentService
    {
        private readonly IGenaricRepo<DriverDocument> _documentRepo;
        private readonly IGenaricRepo<Driver> _driverRepo;

        public DriverDocumentService(
            IGenaricRepo<DriverDocument> documentRepo,
            IGenaricRepo<Driver> driverRepo)
        {
            _documentRepo = documentRepo;
            _driverRepo = driverRepo;
        }

        public async Task<DriverDocumentDto> UploadDocumentAsync(Guid driverId, AddDriverDocumentDto documentDto)
        {
            var driver = await _driverRepo.GetByIdAsync(driverId);
            if (driver == null)
                throw new KeyNotFoundException("Driver not found.");

            var existingDoc = await _documentRepo.GetByAsync(d => d.DriverId == driverId);
            if (existingDoc != null)
            {
                throw new InvalidOperationException("Driver documents already exist. You can update them instead.");
            }

            var document = new DriverDocument
            {
                DriverId = driverId,
                LicenseNumber = documentDto.LicenseNumber,
                CarModel = documentDto.CarModel,
                CarPlateNumber = documentDto.CarPlateNumber,
                CarCategory = documentDto.CarCategory,
                NationalIdImageUrl = documentDto.NationalIdImageUrl,
                DriverLicenseImageUrl = documentDto.DriverLicenseImageUrl,
                CarLicenseImageUrl = documentDto.CarLicenseImageUrl,
                Status = Driveracceptstatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow // Consistent across the project
            };

            await _documentRepo.AddAsync(document);
            await _documentRepo.SaveChangesAsync();

            return MapToDto(document);
        }

        public async Task<DriverDocumentDto> GetDocumentByIdAsync(int documentId)
        {
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new KeyNotFoundException("Driver documents not found.");
            }

            return MapToDto(document);
        }

        public async Task<DriverDocumentDto> GetDocumentsByDriverIdAsync(Guid driverId)
        {
            var document = await _documentRepo.GetByAsync(d => d.DriverId == driverId);
            if (document == null)
            {
                throw new KeyNotFoundException("Driver documents not found.");
            }

            return MapToDto(document);
        }

        public async Task<DriverDocumentDto> UpdateDocumentStatusAsync(int documentId, UpdateDriverDocumentStatusDto statusDto)
        {
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new KeyNotFoundException("Driver documents not found.");
            }

            document.Status = statusDto.Status;

            if (statusDto.Status == Driveracceptstatus.Rejected)
            {
                if (string.IsNullOrWhiteSpace(statusDto.RejectionReason))
                {
                    throw new ArgumentException("Rejection reason is required when rejecting documents.");
                }
                document.RejectionReason = statusDto.RejectionReason;
            }
            else
            {
                document.RejectionReason = null;
            }

            _documentRepo.Update(document);
            await _documentRepo.SaveChangesAsync();

            return MapToDto(document);
        }

        public async Task<bool> DeleteDocumentAsync(Guid driverId, int documentId)
        {
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new KeyNotFoundException("Driver documents not found.");
            }

            // Ownership check
            if (document.DriverId != driverId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this document.");
            }

            // Business Rule: Cannot delete approved documents
            if (document.Status == Driveracceptstatus.Approved)
            {
                throw new InvalidOperationException("Cannot delete approved documents. Please contact support.");
            }

            _documentRepo.Delete(document);
            await _documentRepo.SaveChangesAsync();

            return true;
        }

        private static DriverDocumentDto MapToDto(DriverDocument doc)
        {
            return new DriverDocumentDto
            {
                Id = doc.Id,
                DriverId = doc.DriverId,
                LicenseNumber = doc.LicenseNumber,
                CarModel = doc.CarModel,
                CarPlateNumber = doc.CarPlateNumber,
                CarCategory = doc.CarCategory,
                NationalIdImageUrl = doc.NationalIdImageUrl,
                DriverLicenseImageUrl = doc.DriverLicenseImageUrl,
                CarLicenseImageUrl = doc.CarLicenseImageUrl,
                Status = doc.Status.ToString(),
                RejectionReason = doc.RejectionReason,
                UploadedAt = doc.CreatedAt
            };
        }
    }
}
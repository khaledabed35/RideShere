using DAL.Models;
using System;

namespace BLL.DTOs
{
    public class AddDriverDocumentDto
    {
        public string LicenseNumber { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string CarPlateNumber { get; set; } = string.Empty;
        public string CarCategory { get; set; } = string.Empty;
        public string NationalIdImageUrl { get; set; } = string.Empty;
        public string DriverLicenseImageUrl { get; set; } = string.Empty;
        public string CarLicenseImageUrl { get; set; } = string.Empty;
    }

    public class DriverDocumentDto
    {
        public int Id { get; set; }
        public Guid DriverId { get; set; }

        public string LicenseNumber { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string CarPlateNumber { get; set; } = string.Empty;
        public string CarCategory { get; set; } = string.Empty;

        public string NationalIdImageUrl { get; set; } = string.Empty;
        public string DriverLicenseImageUrl { get; set; } = string.Empty;
        public string CarLicenseImageUrl { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
    }
    public class UpdateDriverDocumentStatusDto
    {
        public Driveracceptstatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}
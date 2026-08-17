using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.DriverDTO
{
    public class DriverDetailsDto
    {
        public Guid DriverId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }

        public string LicenseNumber { get; set; }
        public string CarModel { get; set; }
        public string CarPlateNumber { get; set; }
        public string CarCategory { get; set; }
        public string NationalIdImageUrl { get; set; }
        public string DriverLicenseImageUrl { get; set; }
        public string CarLicenseImageUrl { get; set; }
    }
}

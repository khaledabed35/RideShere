using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.DriverDTO
{
    public class PendingDriverDto
    {
        public Guid DriverId { get; set; } = Guid.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
    }
}

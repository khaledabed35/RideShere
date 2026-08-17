using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.DriverDTO
{
    public class AvailableDriverDto
    {
        public Guid DriverId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        public double DistanceInKm { get; set; } 

        public string? CarModel { get; set; }     
        public string? CarPlateNumber { get; set; }
    }
}

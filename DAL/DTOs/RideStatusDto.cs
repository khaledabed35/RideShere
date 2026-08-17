using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class RideStatusDto
    {
        public Guid RideId { get; set; }
        public string Status { get; set; } = string.Empty; 
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public string carModel { get; set; }
        public string DriverImage { get; set; }
        public decimal DriverRating { get; set; }
    }
}

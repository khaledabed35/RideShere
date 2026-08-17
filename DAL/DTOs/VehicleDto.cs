using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty; 
        public string PlateNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CarImageUrl { get; set; } = string.Empty;
    }
}

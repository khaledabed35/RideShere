using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.DTOs
{
    public class AddVehicleDto
    {
        [Required]
        public Guid DriverId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public string Color { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string CarImageUrl { get; set; } = string.Empty;
    }
}

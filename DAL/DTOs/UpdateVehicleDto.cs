using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.DTOs
{
    public class UpdateVehicleDto
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public string? Brand { get; set; }

        [MaxLength(50)]
        public string? Model { get; set; }

        public string Color { get; set; }

        [MaxLength(20)]
        public string? PlateNumber { get; set; }

        [MaxLength(30)]
        public string? Category { get; set; }

        public string? CarImageUrl { get; set; }
    }
}

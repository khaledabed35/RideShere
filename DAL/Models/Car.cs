using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text;

namespace DAL.Models
{
    public class Car
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Brand { get; set; }
        public string Color { get; set; }
        [Required]
        public Guid DriverId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CarImageUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty; 

        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Category { get; set; } = string.Empty; 

        // --- Navigation Properties ---

        [ForeignKey(nameof(DriverId))]
        public virtual Driver Driver { get; set; } = null!;
    }
}

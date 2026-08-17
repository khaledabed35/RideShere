using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public enum DriverStatus
    {
        Offline,    
        Online,     
        OnTrip      
    }

    [Table("Drivers")]
    public class Driver
    {
        [Key]
        [ForeignKey(nameof(User))]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public String Email { get; set; }
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;
        public virtual DriverDocument? DriverDocument { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public DriverStatus Status { get; set; } = DriverStatus.Offline; 

        [Required]
        [Column(TypeName = "decimal(3, 2)")]
        public decimal Rating { get; set; } = 5.00m;

        public DateTimeOffset? VerifiedAt { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; } 
        public virtual App_User User { get; set; } = null!;
    }
}
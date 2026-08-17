using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DriverId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TripId { get; set; } 

        [Required]
        public Guid ReviewerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")] 
        public int Rating { get; set; } // عدد النجوم من 1 لـ 5

        [MaxLength(500)] 
        public string? Comment { get; set; } 

        [Required]

        // --- Navigation Properties ---

        [ForeignKey(nameof(TripId))]
        public virtual Trip Trip { get; set; } = null!;

        [ForeignKey(nameof(ReviewerId))]
        public virtual App_User Reviewer { get; set; } = null!;
    }
}
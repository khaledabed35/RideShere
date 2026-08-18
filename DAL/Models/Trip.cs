using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace DAL.Models
{
    public enum TripStatus
    {
        Requested,
        Accepted,
        Arrived,
        InProgress,
        Completed,
        Cancelled
    }

    [Table("Trips")]
    public class Trip
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PassengerId { get; set; }

        public Guid? DriverId { get; set; }

        public Guid? CarId { get; set; }

        [Required]
        public Point PickupLocation { get; set; } = null!;

        [Required]
        public Point DropoffLocation { get; set; } = null!;

        [ConcurrencyCheck]
        [Required]
        [Column(TypeName = "varchar(20)")]
        public TripStatus Status { get; set; } = TripStatus.Requested;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal EstimatedFare { get; set; } = 0.00m;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AgreedFare { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? FinalFare { get; set; } = null;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? CompletedAt { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<TripStatusLog> TripStatusLogs { get; set; } = new List<TripStatusLog>();

        // أضف هذا السطر هنا ليختفي الخطأ نهائياً
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();

        [ForeignKey(nameof(PassengerId))]
        public virtual App_User Passenger { get; set; } = null!;

        [ForeignKey(nameof(DriverId))]
        public virtual Driver Driver { get; set; }

        [ForeignKey(nameof(CarId))]
        public virtual Car? Car { get; set; }
    }
}
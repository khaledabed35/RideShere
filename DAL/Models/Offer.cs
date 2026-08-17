using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public enum OfferStatus
    {
        Pending,   
        Accepted,
        Rejected
    }

    [Table("Offers")]
    public class Offer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TripId { get; set; } // الرحلة المرتبطة بالعرض

        [Required]
        public Guid DriverId { get; set; } // السواق اللي بعت العرض

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OfferedPrice { get; set; } // السعر اللي السواق عرضه

        [Required]
        [Column(TypeName = "varchar(20)")]
        public OfferStatus Status { get; set; } = OfferStatus.Pending;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // --- Navigation Properties ---
        [ForeignKey(nameof(TripId))]
        public virtual Trip Trip { get; set; } = null!;

        [ForeignKey(nameof(DriverId))]
        public virtual Driver Driver { get; set; } = null!;
    }
}
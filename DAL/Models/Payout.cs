using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public enum PayoutStatus
    {
        Pending,
        Paid,
        Failed
    }

    [Table("Payouts")]
    public class Payout
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PaymentId { get; set; }

        [Required]
        public Guid DriverId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

        [Required]
        public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;

        // --- Navigation Properties ---
        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; } = null!;

        [ForeignKey(nameof(DriverId))]
        public virtual Driver Driver { get; set; } = null!;
    }
}
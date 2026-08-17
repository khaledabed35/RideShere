using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }
    public enum PaymentMethod
    {
        Cash,
        Paymob
    }

    [Table("Payments")] 
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TripId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(100)]
        public string? GatewayOrderId { get; set; }        // يُحفظ عند إنشاء الطلب (Initiate)
        public string? GatewayTransactionId { get; set; } // يُحفظ عند استقبال الـ Webhook وقت الدفع الفعلي

        public string? GatewayTxnId { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash; // إضافة طريقة الدفع
        [Required]
        public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;

        // --- Navigation Properties ---
        [ForeignKey(nameof(TripId))]
        public virtual Trip Trip { get; set; } = null!;
    }
}
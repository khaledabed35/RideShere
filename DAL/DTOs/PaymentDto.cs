using DAL.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid RideId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }  
        public Status Status { get; set; } 
        public string? TransactionId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }


    }
    public class ChoosePaymentMethodDto
    {
        public Guid TripId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class InitiatePaymobDto
    {
        public Guid TripId { get; set; }
        public decimal Amount { get; set; }
        public string PassengerEmail { get; set; } = string.Empty;
        public string PassengerPhone { get; set; } = string.Empty;
    }
}

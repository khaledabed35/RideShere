using System;
using DAL.Models;

namespace DAL.DTOs
{
    public class TripHistoryDto
    {
        public Guid TripId { get; set; }

        public Guid PassengerId { get; set; }
        public Guid DriverId { get; set; }

        public string PassengerName { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;

        public string DriverImage { get; set; } = string.Empty;

        public string PickupLocation { get; set; } = string.Empty;
        public string DestinationLocation { get; set; } = string.Empty;

        public decimal Fare { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public double Distance { get; set; }

        public int? DurationInMinutes { get; set; }

        public decimal? Rating { get; set; }
    }
}
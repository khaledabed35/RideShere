using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class GeneralNotificationDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class RideRequestNotificationDto
    {
        public Guid DriverId { get; set; }
        public Guid TripId { get; set; }
        public string PickupLocation { get; set; } = string.Empty;
    }

    public class TripStatusNotificationDto
    {
        public Guid TripId { get; set; }

        public Guid UserId { get; set; }
        public string TripStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class DriverOfferNotificationDto
    {
        public Guid PassengerId { get; set; }
        public Guid TripId { get; set; }
        public Guid OfferId { get; set; }
    }

    public class OfferAcceptedNotificationDto
    {
        public Guid DriverId { get; set; }
        public Guid TripId { get; set; }
        public Guid OfferId { get; set; }
    }

    public class PaymentNotificationDto
    {
        public Guid UserId { get; set; }
        public Guid TripId { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
    public class NotificationResultDto
    {
        public Guid TripId { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}

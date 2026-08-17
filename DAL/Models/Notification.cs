using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public App_User? User { get; set; }

        public Guid? TripId { get; set; }
        public Guid? OfferId { get; set; }

        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class TripStatusLogDto
    {
        public Guid Id { get; set; }

        public Guid TripId { get; set; }

        public TripStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? ChangedByUserId { get; set; }

        public string? ChangedByUserName { get; set; }

        public string? Note { get; set; }
    }
}
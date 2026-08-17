using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.DriverDTO
{
    public class DriverReviewDto
    {
        public string PassengerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.DriverDTO
{
    public class DriverEarningsDto
    {
        public Guid DriverId { get; set; }
        public decimal TotalEarnings { get; set; }        
        public int CompletedRidesCount { get; set; }       
        public decimal TodayEarnings { get; set; }       
        public decimal ThisWeekEarnings { get; set; }      

        public IEnumerable<CompletedRideEarningItemDto> RecentCompletedRides { get; set; } = new List<CompletedRideEarningItemDto>();
    }
    public class CompletedRideEarningItemDto
    {
        public Guid RideId { get; set; }
        public string PickupLocation { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Fare { get; set; }
        public DateTimeOffset CompletedAt { get; set; }
    }
}

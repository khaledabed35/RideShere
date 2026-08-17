using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class RideRequestDto
    {
        public Guid RideId { get; set; }
        public Guid passengerid { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string PickupLocation { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal PickupLatitude { get; set; }
        public decimal PickupLongitude { get; set; }
        public decimal DestinationLatitude { get; set; }
        public decimal DestinationLongitude { get; set; }
        public decimal EstimatedFare { get; set; }
        public double DistanceInKm { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal ProposedFare { get; set; }
    }
}

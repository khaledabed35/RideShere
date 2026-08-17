using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.Auth
{
    public class RequestRideDto
    {
        public decimal PickupLatitude { get; set; }
        public decimal PickupLongitude { get; set; }
        public decimal DestinationLatitude { get; set; }
        public decimal DestinationLongitude { get; set; }
        public string CarType { get; set; }
        public decimal PassengerProposedFare { get; set; } 
    }

}

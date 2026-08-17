using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class RideEstimateDto
    {
        public decimal EstimatedFare { get; set; }         
        public double EstimatedDistanceInKm { get; set; }   

        public int EstimatedDurationInMinutes { get; set; } 
    }
}

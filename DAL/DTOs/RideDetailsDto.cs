using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class RideDetailsDto
    {
    public Guid RideId { get; set; }
    public string Status { get; set; } = string.Empty; 
    public string PickupLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal FinalFare { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public double DriverRating { get; set; }
    public string? CarDetails { get; set; }
        public string  message { get; set; }

        public IEnumerable<DriverOfferDto> Offers { get; set; } = new List<DriverOfferDto>();

}
}

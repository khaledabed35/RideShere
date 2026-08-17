public class DriverOfferDto
{
    public Guid OfferId { get; set; }

    public Guid DriverId { get; set; }

    public string DriverName { get; set; } = string.Empty;
    public string DriverImage { get; set; } = string.Empty;
    public decimal Rating { get; set; }

    public string CarModel { get; set; } = string.Empty;
    public string CarPlateNumber { get; set; } = string.Empty;
    public string CarCategory { get; set; } = string.Empty;
    public string CarImageUrl { get; set; } = string.Empty;

    public decimal ProposedFare { get; set; }

    public decimal DriverLatitude { get; set; }
    public decimal DriverLongitude { get; set; }

    public double DistanceFromPassenger { get; set; }
    public int EstimatedArrivalMinutes { get; set; }

    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
namespace DAL.DTOs.DriverDTO
{
    public class ProposeFareDto
    {
        public decimal NewFare { get; set; }
    }

    public class UpdateAvailabilityDto
    {
        public bool IsOnline { get; set; }
    }

    public class UpdateLocationDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class UpdateRideStatusDto
    {
        public string Status { get; set; } = string.Empty; // مثل: Arrived, InProgress, Completed
    }
}
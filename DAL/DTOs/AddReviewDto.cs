using DAL.Models;
using Nest;
using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class AddReviewDto
    {
        public Guid TripId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; } 

        public string? Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public class DriverReviewDto
    {
        public Guid Id { get; set; }
        public Guid PassengerId { get; set; }
        public Guid DriverId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
    }

    public class DriverRatingDto
    {
        public Guid DriverId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviewsCount { get; set; }
    }
}
using BLL.DTOs;
using DAL.Models;
using DAL.Reposetoriy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class ReviewService : IReviewService
    {
        private readonly IGenaricRepo<Trip> _trip;
        private readonly IGenaricRepo<Review> _review;

        public ReviewService(
            IGenaricRepo<Trip> trip,
            IGenaricRepo<Review> review)
        {
            _trip = trip;
            _review = review;
        }

        public async Task<DriverReviewDto> AddReviewAsync(Guid passengerId, AddReviewDto reviewDto)
        {
            var trip = await _trip.GetByIdAsync(reviewDto.TripId);
            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found.");
            }

            if (trip.PassengerId != passengerId)
            {
                throw new UnauthorizedAccessException("You are not authorized to review this trip.");
            }

            if (trip.Status != TripStatus.Completed)
            {
                throw new InvalidOperationException("You can only review completed trips.");
            }

            if (trip.DriverId == Guid.Empty)
            {
                throw new InvalidOperationException("This trip does not have a driver.");
            }

            var existingReview = await _review.GetByAsync(r => r.TripId == reviewDto.TripId && r.ReviewerId == passengerId);
            if (existingReview != null)
            {
                throw new InvalidOperationException("You have already reviewed this trip.");
            }

            var review = new Review
            {
                Id = Guid.NewGuid(),
                TripId = reviewDto.TripId,
                ReviewerId = passengerId,
                DriverId = trip.DriverId,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _review.AddAsync(review);
            await _review.SaveChangesAsync();

            return new DriverReviewDto
            {
                Id = review.Id,
                PassengerId = review.ReviewerId,
                DriverId = review.DriverId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<bool> DeleteReviewAsync(Guid passengerId, Guid reviewId)
        {
            var review = await _review.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException("Review not found.");

            if (review.ReviewerId != passengerId)
                throw new UnauthorizedAccessException("You are not authorized to delete this review.");

            _review.Delete(review);
            await _review.SaveChangesAsync();

            return true;
        }

        public async Task<DriverRatingDto> GetDriverRatingAsync(Guid driverId)
        {
            var spec = new DriverReviewsSpecification(driverId);
            var driverReviews = (await _review.GetAllWithSpecAsync(spec)).ToList();

            if (!driverReviews.Any())
            {
                return new DriverRatingDto
                {
                    DriverId = driverId,
                    AverageRating = 0.0,
                    TotalReviewsCount = 0
                };
            }

            return new DriverRatingDto
            {
                DriverId = driverId,
                AverageRating = Math.Round(driverReviews.Average(r => r.Rating), 1),
                TotalReviewsCount = driverReviews.Count
            };
        }

        public async Task<IEnumerable<DriverReviewDto>> GetReviewsByDriverIdAsync(Guid driverId)
        {
            var spec = new DriverReviewsSpecification(driverId);
            var reviews = await _review.GetAllWithSpecAsync(spec);

            return reviews.Select(review => new DriverReviewDto
            {
                Id = review.Id,
                PassengerId = review.ReviewerId,
                DriverId = review.DriverId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            }).ToList();
        }

        public async Task<DriverReviewDto> UpdateReviewAsync(Guid passengerId, Guid reviewId, UpdateReviewDto reviewDto)
        {
            var review = await _review.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException("Review not found.");

            if (review.ReviewerId != passengerId)
                throw new UnauthorizedAccessException("You are not authorized to update this review.");

            review.Rating = reviewDto.Rating;
            review.Comment = reviewDto.Comment;

            _review.Update(review);
            await _review.SaveChangesAsync();

            return new DriverReviewDto
            {
                Id = review.Id,
                PassengerId = review.ReviewerId,
                DriverId = review.DriverId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
}
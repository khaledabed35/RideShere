using BLL.DTOs;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewDto reviewDto)
        {
            var passengerId = GetUserIdFromClaims();
            var result = await _reviewService.AddReviewAsync(passengerId, reviewDto);

            return CreatedAtAction(
                nameof(GetReviewsByDriver),
                new { driverId = result.DriverId },
                result);
        }

        [HttpPut("{reviewId}")]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] UpdateReviewDto reviewDto)
        {
            var passengerId = GetUserIdFromClaims();
            var result = await _reviewService.UpdateReviewAsync(passengerId, reviewId, reviewDto);
            return Ok(result);
        }

        [HttpDelete("{reviewId}")]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            var passengerId = GetUserIdFromClaims();
            await _reviewService.DeleteReviewAsync(passengerId, reviewId);
            return NoContent();
        }

        [HttpGet("driver/{driverId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsByDriver(Guid driverId)
        {
            var reviews = await _reviewService.GetReviewsByDriverIdAsync(driverId);
            return Ok(reviews);
        }

        [HttpGet("driver/{driverId}/rating")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDriverRating(Guid driverId)
        {
            var rating = await _reviewService.GetDriverRatingAsync(driverId);
            return Ok(rating);
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.FindFirst("uid")?.Value;

            if (!Guid.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user token.");
            }

            return userId;
        }
    }
}
using BLL.Services.Interface;
using DAL.DTOs;
using DAL.DTOs.Auth;
using DAL.DTOs.DriverDTO; // Make sure this includes your required DTOs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Passenger,Admin")]
    public class PassengerController : ControllerBase
    {
        private readonly IPassengerService _passenger;

        public PassengerController(IPassengerService passenger)
        {
            _passenger = passenger;
        }

        [HttpPost("request-ride")]
        public async Task<IActionResult> RequestRide([FromBody] RequestRideDto model)
        {
            var passengerId = GetUserIdFromClaims();
            if (passengerId == null) return Unauthorized();

            var result = await _passenger.RequestRideAsync(passengerId.Value, model);
            return Ok(result);
        }

        [HttpGet("current-trip")]
        public async Task<IActionResult> GetCurrentTrip()
        {
            var passengerId = GetUserIdFromClaims();
            if (passengerId == null) return Unauthorized();

            var result = await _passenger.GetCurrentTripAsync(passengerId.Value);
            if (result == null)
                return NotFound(new { message = "No active trip found." });

            return Ok(result);
        }

        [HttpGet("nearby-drivers")]
        public async Task<IActionResult> GetNearbyDrivers([FromQuery] decimal latitude, [FromQuery] decimal longitude)
        {
            var drivers = await _passenger.GetNearbyDriversAsync(latitude, longitude);
            return Ok(drivers);
        }

        [HttpGet("trips/{tripId}/offers")]
        public async Task<IActionResult> GetTripOffers(Guid tripId)
        {
            var passengerId = GetUserIdFromClaims();
            if (passengerId == null) return Unauthorized();

            try
            {
                var offers = await _passenger.GetTripOffersAsync(passengerId.Value, tripId);
                return Ok(offers);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("trips/{tripId}/accept-offer/{offerId}")]
        public async Task<IActionResult> AcceptDriverOffer(Guid tripId, Guid offerId)
        {
            var passengerId = GetUserIdFromClaims();
            if (passengerId == null) return Unauthorized();

            try
            {
                await _passenger.AcceptDriverOfferAsync(passengerId.Value, offerId);
                return Ok(new { message = "Offer accepted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("trips/{tripId}/cancel")]
        public async Task<IActionResult> CancelTrip(Guid tripId, [FromBody] CancelTripDto model)
        {
            var passengerId = GetUserIdFromClaims();
            if (passengerId == null) return Unauthorized();

            try
            {
                await _passenger.CancelTripAsync(passengerId.Value, tripId, model.Reason);
                return Ok(new { message = "Trip cancelled successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("trips/{tripId}/review")]
        public async Task<IActionResult> AddTripReview(Guid tripId, [FromBody] DriverReviewDto model)
        {
            var passengerId = GetUserIdFromClaims();
            if (passengerId == null) return Unauthorized();

            try
            {
                await _passenger.AddTripReviewAsync(passengerId.Value, tripId, model.Comment);
                return Ok(new { message = "Review added successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid? GetUserIdFromClaims()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdStr, out var userGuid))
            {
                return userGuid;
            }
            return null;
        }
    }
}
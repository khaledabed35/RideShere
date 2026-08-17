using BLL.Services.Interface; // أو مساحة الأسماء الخاصة بـ IDriverService لديك
using DAL.DTOs;
using DAL.DTOs.DriverDTO; // تأكد من مطابقة مساحات الأسماء لديك
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Driver")]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet("available-rides")]
        public async Task<IActionResult> GetAvailableRideRequests([FromQuery] decimal latitude, [FromQuery] decimal longitude)
        {
            try
            {
                var rides = await _driverService.GetAvailableRideRequestsAsync(latitude, longitude);
                return Ok(rides);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("current-ride")]
        public async Task<IActionResult> GetCurrentRide()
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            var ride = await _driverService.GetCurrentRideAsync(driverId.Value);
            if (ride == null)
            {
                return NotFound(new { message = "No active ride found." });
            }

            return Ok(ride);
        }

        [HttpPost("rides/{rideId}/accept")]
        public async Task<IActionResult> AcceptRide(Guid rideId)
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            try
            {
                await _driverService.AcceptRideAsync(driverId.Value, rideId);
                return Ok(new { message = "Ride accepted successfully." });
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

        [HttpPost("rides/{rideId}/reject")]
        public async Task<IActionResult> RejectRide(Guid rideId)
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            try
            {
                await _driverService.RejectRideAsync(driverId.Value, rideId);
                return Ok(new { message = "Ride rejected successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("rides/{rideId}/propose-fare")]
        public async Task<IActionResult> ProposeNewFare(Guid rideId, [FromBody] ProposeFareDto model)
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            try
            {
                await _driverService.ProposeNewFareAsync(driverId.Value, rideId, model.NewFare);
                return Ok(new { message = "Fare proposed successfully." });
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

        [HttpPatch("status")]
        public async Task<IActionResult> UpdateAvailability([FromBody] UpdateAvailabilityDto model)
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            try
            {
                await _driverService.UpdateDriverAvailabilityAsync(driverId.Value, model.IsOnline);
                return Ok(new { message = "Driver availability updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("location")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationDto model)
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            await _driverService.UpdateDriverLocationAsync(driverId.Value, model.Latitude, model.Longitude);
            return Ok(new { message = "Location updated successfully in Redis." });
        }

        [HttpPatch("rides/{rideId}/status")]
        public async Task<IActionResult> UpdateRideStatus(Guid rideId, [FromBody] UpdateRideStatusDto model)
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            try
            {
                await _driverService.UpdateRideStatusByDriverAsync(driverId.Value, rideId, model.Status);
                return Ok(new { message = "Ride status updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("earnings")]
        public async Task<IActionResult> GetEarnings()
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            var earnings = await _driverService.GetEarningsAsync(driverId.Value);
            return Ok(earnings);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetRideHistory([FromQuery] string filterPeriod = "all")
        {
            var driverId = GetUserIdFromClaims();
            if (driverId == null) return Unauthorized();

            var history = await _driverService.GetDriverRideHistoryAsync(driverId.Value, filterPeriod);
            return Ok(history);
        }

        private Guid? GetUserIdFromClaims()
        {
            var userIdStr = User.FindFirst("uid")?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdStr, out var userGuid))
            {
                return userGuid;
            }
            return null;
        }
    }
}
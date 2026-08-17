using BLL.Services.Interface;
using DAL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly IRideService _rideService;

        public TripController(IRideService rideService)
        {
            _rideService = rideService;
        }

        /// <summary>
        /// جلب الرحلة النشطة الحالية (للراكب أو السائق)
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<RideDetailsDto>> GetActiveTrip()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token." });

            var activeTrip = await _rideService.GetActiveTripAsync(userId.Value);
            if (activeTrip == null)
                return NotFound(new { message = "No active trip found." });

            return Ok(activeTrip);
        }

        /// <summary>
        /// جلب تفاصيل رحلة معينة باستخدام الـ TripId
        /// </summary>
        [HttpGet("{tripId:guid}")]
        public async Task<ActionResult<RideDetailsDto>> GetTripDetails(Guid tripId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token." });

            var tripDetails = await _rideService.GetTripDetailsAsync(tripId, userId.Value);
            return Ok(tripDetails);
        }

        /// <summary>
        /// جلب تاريخ رحلات المستخدم (راكب أو سائق)
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<TripHistoryDto>>> GetTripHistory()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token." });

            var history = await _rideService.GetTripHistoryAsync(userId.Value);
            return Ok(history);
        }

        /// <summary>
        /// جلب سجل حالات الرحلة
        /// </summary>
        [HttpGet("{tripId:guid}/status-history")]
        public async Task<ActionResult<IEnumerable<TripStatusLogDto>>> GetTripStatusHistory(Guid tripId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token." });

            var statusHistory = await _rideService.GetTripStatusHistoryAsync(tripId, userId.Value);
            return Ok(statusHistory);
        }

        private Guid? GetUserId()
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
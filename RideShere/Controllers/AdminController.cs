using BLL.Services.Interface;
using BLL.Specification.Class;
using DAL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace RideShere.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet("drivers/{driverId}")]
        public async Task<IActionResult> GetDriverDetails([FromRoute] Guid driverId)
        {
            if (driverId == Guid.Empty)
                return BadRequest("Driver ID is required.");

            var driverDetails = await _adminUserService.GetDriverDetailsForAdminAsync(driverId);
            return Ok(driverDetails);
        }

        [HttpPatch("drivers/{driverId}/approve")]
        public async Task<IActionResult> ApproveDriver([FromRoute] Guid driverId)
        {
            if (driverId == Guid.Empty)
                return BadRequest("Driver ID is required.");

            await _adminUserService.ApproveDriverAsync(driverId);
            return Ok(new { message = "Driver approved successfully." });
        }

        [HttpPatch("drivers/{driverId}/reject")]
        public async Task<IActionResult> RejectDriver([FromRoute] Guid driverId, [FromBody] RejectDriverDto dto)
        {
            if (driverId == Guid.Empty)
                return BadRequest("Driver ID is required.");

            if (dto == null || string.IsNullOrWhiteSpace(dto.RejectionReason))
                return BadRequest("Rejection reason is required.");

            await _adminUserService.RejectDriverAsync(driverId, dto.RejectionReason);
            return Ok(new { message = "Driver rejected successfully." });
        }

        [HttpGet("drivers/pending")]
        public async Task<IActionResult> GetPendingDrivers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pendingDrivers = await _adminUserService.GetPendingDriversQueueAsync(pageNumber, pageSize);
            return Ok(pendingDrivers);
        }

        [HttpGet("users")]
        public async Task<IActionResult> ListAllUsers([FromQuery] UserQueryParameters queryParams)
        {
            var users = await _adminUserService.ListAllUsersAsync(queryParams);
            return Ok(users);
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("User ID is required.");

            var user = await _adminUserService.GetUserByIdAsync(userId);
            return Ok(user);
        }

        [HttpPatch("users/{userId}/suspend")]
        public async Task<IActionResult> SuspendUser([FromRoute] Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("User ID is required.");

            await _adminUserService.SuspendUserAsync(userId);
            return Ok(new { message = "User suspended successfully." });
        }

        [HttpPatch("users/{userId}/unblock")]
        public async Task<IActionResult> UnBlockUser([FromRoute] Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("User ID is required.");

            await _adminUserService.UnBlockUserAsync(userId);

            return Ok(new { message = "User unblocked successfully." });
        }
    }
}
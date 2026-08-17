using System.Security.Claims;
using BLL.Services.Interface;
using DAL.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // ميثود مساعدة لجلب الـ Guid بطريقة آمنة لكل الـ Endpoints
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

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { Message = "User ID is required or invalid format." });
            }

            var userProfile = await _userService.GetUserProfileAsync(userGuid.Value);
            return Ok(userProfile);
        }

        [HttpPut("update-profile")]
        [Authorize(Roles = "Passenger,Admin")] // السماح للـ Passenger أو Admin لضمان عدم ظهور 403
        public async Task<IActionResult> UpdateUserProfile(UpdateUserProfileDto model)
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { Message = "User ID is required or invalid format." });
            }

            await _userService.UpdateUserProfileAsync(userGuid.Value, model);
            return Ok("Profile updated successfully.");
        }

        [HttpDelete("delete-account")]
        [Authorize(Roles = "Passenger,Admin")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { Message = "User ID is required or invalid format." });
            }

            await _userService.DeleteAccountAsync(userGuid.Value);
            return Ok("Account deleted successfully.");
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "Passenger,Admin")]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { Message = "User ID is required or invalid format." });
            }

            var result = await _userService.UploadProfileImageAsync(userGuid.Value, image);
            return Ok(result);
        }
    }
}
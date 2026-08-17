using System.Security.Claims;
using BLL.Services.Interface;
using DAL.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Passenger,Admin")] // تحديث الصلاحيات لتتوافق مع الأدوار الموجودة لديك
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // ميثود مساعدة لجلب الـ Guid بطريقة آمنة لجميع الـ Endpoints
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

        [HttpGet("GetFavoritePlaces")]
        public async Task<IActionResult> GetFavoritePlaces([FromQuery] string? searchTerm)
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { message = "Valid User ID is required." });
            }

            var addresses = await _addressService.GetFavoritePlacesAsync(userGuid.Value, searchTerm);
            return Ok(addresses);
        }

        [HttpGet("GetFavoritePlaceById/{id}")]
        public async Task<IActionResult> GetFavoritePlaceById(int id)
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { message = "Valid User ID is required." });
            }

            try
            {
                var address = await _addressService.GetFavoritePlaceByIdAsync(userGuid.Value, id);
                return Ok(address);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("AddFavoritePlace")]
        public async Task<IActionResult> AddFavoritePlace([FromBody] FavoritePlaceDto model)
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { message = "Valid User ID is required." });
            }

            var result = await _addressService.AddFavoritePlaceAsync(userGuid.Value, model);
            return Ok(new { message = result });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFavoritePlace(int id, [FromBody] FavoritePlaceDto model)
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { message = "Valid User ID is required." });
            }

            try
            {
                var updatedAddress = await _addressService.UpdateFavoritePlaceAsync(userGuid.Value, id, model);
                return Ok(updatedAddress);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavoritePlace(int id)
        {
            var userGuid = GetUserIdFromClaims();
            if (userGuid == null)
            {
                return Unauthorized(new { message = "Valid User ID is required." });
            }

            try
            {
                await _addressService.DeleteFavoritePlaceAsync(userGuid.Value, id);
                return Ok(new { message = "Favorite place deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
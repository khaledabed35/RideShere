using BLL.Services.Interface;
using DAL.DTOs;
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
    [Authorize] // يتطلب تسجيل الدخول
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        /// <smary>
        /// إضافة مركبة جديدة للسائق الحالي
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddVehicle([FromBody] AddVehicleDto model)
        {
            var driverId = GetCurrentUserId();
            if (driverId == null) return Unauthorized();

            try
            {
                var vehicle = await _vehicleService.AddVehicleAsync(driverId.Value, model);
                return CreatedAtAction(nameof(GetMyVehicle), new { }, vehicle);
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

        /// <summary>
        /// جلب تفاصيل مركبة السائق الحالي
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyVehicle()
        {
            var driverId = GetCurrentUserId();
            if (driverId == null) return Unauthorized();

            var vehicle = await _vehicleService.GetMyVehicleAsync(driverId.Value);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found for this driver." });
            }

            return Ok(vehicle);
        }

        /// <summary>
        /// تعديل بيانات مركبة السائق الحالي
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleDto model)
        {
            var driverId = GetCurrentUserId();
            if (driverId == null) return Unauthorized();

            try
            {
                var vehicle = await _vehicleService.UpdateVehicleAsync(driverId.Value, model);
                return Ok(vehicle);
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

     
        [HttpDelete]
        public async Task<IActionResult> DeleteVehicle()
        {
            var driverId = GetCurrentUserId();
            if (driverId == null) return Unauthorized();

            try
            {
                await _vehicleService.DeleteVehicleAsync(driverId.Value);
                return Ok(new { message = "Vehicle deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var driverId))
            {
                return driverId;
            }
            return null;
        }
    }
}
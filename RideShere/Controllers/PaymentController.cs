using BLL.Services.Interface;
using DAL.DTOs;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("choose-method")]
        public async Task<IActionResult> ChoosePaymentMethod([FromBody] ChoosePaymentMethodDto model)
        {
            try
            {
                var passengerId = GetUserIdFromClaims();
                if (passengerId == null)
                    return Unauthorized(new { message = "Invalid user token or ID claim not found." });

                await _paymentService.ChoosePaymentMethodAsync(passengerId.Value, model.TripId, model.PaymentMethod);
                return Ok(new { message = "Payment method selected successfully." });
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

        /// <summary>
        /// تأكيد الدفع النقدي (Cash) بواسطة السائق عند انتهاء الرحلة
        /// </summary>
        [HttpPost("complete-cash/{tripId}")]
        public async Task<IActionResult> CompleteCashPayment(Guid tripId)
        {
            try
            {
                var driverId = GetUserIdFromClaims();
                if (driverId == null)
                    return Unauthorized(new { message = "Invalid user token or ID claim not found." });

                var result = await _paymentService.CompleteCashPaymentAsync(tripId, driverId.Value);
                if (!result)
                    return BadRequest(new { message = "Failed to complete cash payment." });

                return Ok(new { message = "Cash payment completed successfully." });
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

        /// <summary>
        /// بدء عملية الدفع عبر بوابة Paymob وإرجاع رابط الدفع (Iframe URL)
        /// </summary>
        [HttpPost("paymob/initiate")]
        public async Task<IActionResult> InitiatePaymobPayment([FromBody] InitiatePaymobDto model)
        {
            try
            {
                string paymentUrl = await _paymentService.InitiatePaymobPaymentAsync(
                    model.TripId,
                    model.Amount,
                    model.PassengerEmail,
                    model.PassengerPhone
                );

                return Ok(new { paymentUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// استقبال الـ Webhook القادم من Paymob لتحديث حالة الدفع تلقائياً
        /// </summary>
        [HttpPost("paymob/webhook")]
        [AllowAnonymous] // يجب أن يكون متاحاً لـ Paymob للوصول إليه بدون توكن
        public async Task<IActionResult> PaymobWebhook([FromBody] object webhookPayload)
        {
            try
            {
                string rawJson = webhookPayload.ToString() ?? string.Empty;

                if (webhookPayload is System.Text.Json.JsonElement jsonElement)
                {
                    rawJson = jsonElement.GetRawText();
                }

                bool success = await _paymentService.ProcessPaymobWebhookAsync(rawJson);

                if (!success)
                    return BadRequest(new { message = "Webhook processed with errors or invalid data." });

                return Ok(new { message = "Webhook processed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// استرجاع أموال الدفع (Refund) لمعاملة تمت مسبقاً (خاص بالمسؤولين)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("refund/{paymentId}")]
        public async Task<IActionResult> RefundPayment(Guid paymentId)
        {
            try
            {
                bool success = await _paymentService.RefundPaymentAsync(paymentId);
                if (!success)
                    return BadRequest(new { message = "Refund process failed. Check payment status or transaction ID." });

                return Ok(new { message = "Payment refunded successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("status/{tripId}")]
        public async Task<IActionResult> GetPaymentStatus(Guid tripId)
        {
            var paymentDto = await _paymentService.GetPaymentStatusAsync(tripId);
            if (paymentDto == null)
                return NotFound(new { message = "Payment record not found for this trip." });

            return Ok(paymentDto);
        }

        /// <summary>
        /// جلب سجل مدفوعات الراكب
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var passengerId = GetUserIdFromClaims();
            if (passengerId == null)
                return Unauthorized(new { message = "Invalid user token or ID claim not found." });

            var history = await _paymentService.GetPaymentHistoryAsync(passengerId.Value);

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
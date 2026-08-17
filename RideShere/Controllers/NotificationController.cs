using BLL.Services.Interface;
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
    [Authorize] // يتطلب أن يكون المستخدم مسجل دخول لتنفيذ هذه العمليات
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value);
            return Ok(notifications);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId.Value);
            return Ok(notifications);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var success = await _notificationService.MarkAsReadAsync(userId.Value, id);
            if (!success)
            {
                return NotFound(new { message = "Notification not found or unauthorized." });
            }

            return Ok(new { message = "Notification marked as read successfully." });
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId.Value);
            return Ok(new { message = "All notifications marked as read." });
        }

        // Helper method لاستخراج الـ Guid الخاص بالمستخدم من الـ Claims بأمان
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
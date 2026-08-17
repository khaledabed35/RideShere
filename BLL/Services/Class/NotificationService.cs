using BLL.Helper;
using BLL.Services.Interface;
using BLL.Specifications;
using DAL.DTOs;
using DAL.Models;
using DAL.Reposetoriy;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<RideHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        private readonly IGenaricRepo<Notification> _notificationRepo;

        public NotificationService(
            IHubContext<RideHub> hubContext,
            ILogger<NotificationService> logger,
            IGenaricRepo<Notification> notificationRepo)
        {
            _hubContext = hubContext;
            _logger = logger;
            _notificationRepo = notificationRepo;
        }

        private async Task<NotificationResultDto> CreateAndSendAsync(
            Guid userId,
            string title,
            string body,
            string type,
            string eventName,
            object payload,
            string successMessage,
            Guid? tripId = null,
            Guid? offerId = null)
        {
            var sentAt = DateTime.UtcNow;

            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Body = body,
                    Type = type,
                    TripId = tripId,
                    OfferId = offerId,
                    IsRead = false,
                    CreatedAt = sentAt
                };

                // قاعدة البيانات هي الأساس (Source of Truth)
                await _notificationRepo.AddAsync(notification);
                await _notificationRepo.SaveChangesAsync();

                // محاولة إرسال الـ Real-time عبر SignalR (فشلها لا يكسر العملية)
                try
                {
                    await _hubContext.Clients
                        .Group(userId.ToString())
                        .SendAsync(eventName, payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Notification {NotificationId} persisted but realtime delivery failed for user {UserId}",
                        notification.Id, userId);
                }

                return new NotificationResultDto
                {
                    IsSuccess = true,
                    Message = successMessage,
                    SentAt = sentAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification for user {UserId}", userId);
                return new NotificationResultDto
                {
                    IsSuccess = false,
                    Message = "Failed to create notification.",
                    SentAt = sentAt
                };
            }
        }

        // 2. تطبيق الـ Methods باستخدام الـ Helper
        public async Task<NotificationResultDto> SendNotificationAsync(GeneralNotificationDto notificationDto)
        {
            var payload = new { Title = notificationDto.Title, Body = notificationDto.Body, SentAt = DateTime.UtcNow };
            return await CreateAndSendAsync(
                notificationDto.UserId,
                notificationDto.Title,
                notificationDto.Body,
                "General",
                "ReceiveGeneralNotification",
                payload,
                "General notification sent successfully."
            );
        }

        public async Task<NotificationResultDto> SendRideRequestNotificationAsync(RideRequestNotificationDto notificationDto)
        {
            var title = "New Ride Request";
            var body = $"You have a new ride request for trip {notificationDto.TripId}";
            var payload = new { notificationDto.TripId, notificationDto.PickupLocation, SentAt = DateTime.UtcNow };

            return await CreateAndSendAsync(
                notificationDto.DriverId,
                title,
                body,
                "RideRequest",
                "ReceiveRideRequestNotification",
                payload,
                "Ride request notification sent to driver.",
                tripId: notificationDto.TripId
            );
        }

        public async Task<NotificationResultDto> SendTripStatusNotificationAsync(TripStatusNotificationDto notificationDto)
        {
            var title = "Trip Status Update";
            var payload = new { notificationDto.TripStatus, notificationDto.Message, SentAt = DateTime.UtcNow };

            return await CreateAndSendAsync(
                notificationDto.UserId,
                title,
                notificationDto.Message,
                "TripStatus",
                "ReceiveTripStatusNotification",
                payload,
                "Trip status notification sent successfully.",
                tripId: notificationDto.TripId
            );
        }

        public async Task<NotificationResultDto> SendDriverOfferNotificationAsync(DriverOfferNotificationDto notificationDto)
        {
            var title = "New Driver Offer";
            var body = $"You received a new offer for trip {notificationDto.TripId}";
            var payload = new { notificationDto.TripId, notificationDto.OfferId, SentAt = DateTime.UtcNow };

            return await CreateAndSendAsync(
                notificationDto.PassengerId,
                title,
                body,
                "DriverOffer",
                "ReceiveDriverOfferNotification",
                payload,
                "Driver offer notification sent to passenger.",
                tripId: notificationDto.TripId,
                offerId: notificationDto.OfferId
            );
        }

        public async Task<NotificationResultDto> SendOfferAcceptedNotificationAsync(OfferAcceptedNotificationDto notificationDto)
        {
            var title = "Offer Accepted";
            var body = $"Your offer for trip {notificationDto.TripId} has been accepted!";
            var payload = new { notificationDto.TripId, notificationDto.OfferId, SentAt = DateTime.UtcNow };

            return await CreateAndSendAsync(
                notificationDto.DriverId,
                title,
                body,
                "OfferAccepted",
                "ReceiveOfferAcceptedNotification",
                payload,
                "Offer accepted notification sent to driver.",
                tripId: notificationDto.TripId,
                offerId: notificationDto.OfferId
            );
        }

        public async Task<NotificationResultDto> SendPaymentNotificationAsync(PaymentNotificationDto notificationDto)
        {
            var title = "Payment Status";
            var body = $"Payment status for trip {notificationDto.TripId}: {notificationDto.PaymentStatus}";
            var payload = new { notificationDto.TripId, notificationDto.PaymentStatus, SentAt = DateTime.UtcNow };

            return await CreateAndSendAsync(
                notificationDto.UserId,
                title,
                body,
                "Payment",
                "ReceivePaymentNotification",
                payload,
                "Payment notification sent successfully.",
                tripId: notificationDto.TripId
            );
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            var spec = new NotificationSpecification(userId);
            return await _notificationRepo.GetAllWithSpecAsync(spec);
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId)
        {
            var spec = new NotificationSpecification(userId, unreadOnly: true);
            return await _notificationRepo.GetAllWithSpecAsync(spec);
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, int notificationId)
        {
            var notification = await _notificationRepo.GetByAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return false; 
            }

            notification.IsRead = true;
            _notificationRepo.Update(notification); // تحديث الحالة
            await _notificationRepo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var spec = new NotificationSpecification(userId, unreadOnly: true);
            var unreadNotifications = await _notificationRepo.GetAllWithSpecAsync(spec);

            if (!unreadNotifications.Any()) return true;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                _notificationRepo.Update(notification);
            }

            await _notificationRepo.SaveChangesAsync();
            return true;
        }
    }
}
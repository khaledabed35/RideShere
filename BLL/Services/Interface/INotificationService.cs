using DAL.DTOs;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface INotificationService
    {
        Task<NotificationResultDto> SendNotificationAsync(GeneralNotificationDto notificationDto);

        Task<NotificationResultDto> SendRideRequestNotificationAsync(RideRequestNotificationDto notificationDto);

        Task<NotificationResultDto> SendTripStatusNotificationAsync(TripStatusNotificationDto notificationDto);

        Task<NotificationResultDto> SendDriverOfferNotificationAsync(DriverOfferNotificationDto notificationDto);

        Task<NotificationResultDto> SendOfferAcceptedNotificationAsync(OfferAcceptedNotificationDto notificationDto);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId);
        Task<NotificationResultDto> SendPaymentNotificationAsync(PaymentNotificationDto notificationDto);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid userId, int notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
    }
}


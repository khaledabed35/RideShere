using DAL.DTOs;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface IPaymentService
    {
        Task<PaymentDto> CreatePaymentRecordAsync(Guid tripId, decimal amount, PaymentMethod method);
        Task<PaymentDto?> GetPaymentStatusAsync(Guid tripId);
        Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync(Guid passengerId);

        Task<string> InitiatePaymobPaymentAsync(Guid tripId, decimal amount, string passengerEmail, string passengerPhone);

        Task<bool> ProcessPaymobWebhookAsync(string callbackData);

        Task<bool> RefundPaymentAsync(Guid paymentId);
        Task<bool> CompleteCashPaymentAsync(Guid tripId, Guid driverId);
        Task ChoosePaymentMethodAsync(Guid passengerId, Guid tripId, PaymentMethod paymentMethod);

    }
}

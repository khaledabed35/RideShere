using BLL.Services.Interface;
using DAL.DTOs;
using DAL.Models;
using DAL.Reposetoriy;
using DAL.Specification.Class;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class PaymentService : IPaymentService
    {
        private readonly IGenaricRepo<Payment> _paymentRepo;
        private readonly IGenaricRepo<Trip> _tripRepo;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PaymentService(
             IGenaricRepo<Payment> paymentRepo,
             IGenaricRepo<Trip> tripRepo,
             HttpClient httpClient,
             IConfiguration configuration)
        {
            _paymentRepo = paymentRepo;
            _tripRepo = tripRepo;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task ChoosePaymentMethodAsync(Guid passengerId, Guid tripId, PaymentMethod paymentMethod)
        {
            var trip = await _tripRepo.GetByAsync(t => t.Id == tripId && t.PassengerId == passengerId);
            if (trip == null)
                throw new KeyNotFoundException("Trip not found or unauthorized.");

            trip.PaymentMethod = paymentMethod;
            _tripRepo.Update(trip);
            await _tripRepo.SaveChangesAsync();

            var existingPayment = await _paymentRepo.GetByAsync(p => p.TripId == tripId);
            decimal amount = trip.AgreedFare > 0 ? trip.AgreedFare : trip.EstimatedFare;

            if (existingPayment == null)
            {
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    TripId = tripId,
                    Amount = amount,
                    Method = paymentMethod,
                    Status = PaymentStatus.Pending,
                    ProcessedAt = DateTimeOffset.UtcNow
                };
                await _paymentRepo.AddAsync(payment);
            }
            else
            {
                existingPayment.Method = paymentMethod;
                existingPayment.Amount = amount;
                _paymentRepo.Update(existingPayment);
            }

            await _paymentRepo.SaveChangesAsync();
        }

        public async Task<bool> CompleteCashPaymentAsync(Guid tripId, Guid driverId)
        {
            var trip = await _tripRepo.GetByAsync(t => t.Id == tripId && t.DriverId == driverId);
            if (trip == null)
                throw new KeyNotFoundException("Trip not found or you are not authorized as the driver for this trip.");

            var payment = await _paymentRepo.GetByAsync(p => p.TripId == tripId);

            if (payment == null)
            {
                decimal amount = trip.AgreedFare > 0 ? trip.AgreedFare : trip.EstimatedFare;

                payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    TripId = tripId,
                    Amount = amount,
                    Method = PaymentMethod.Cash,
                    Status = PaymentStatus.Completed,
                    ProcessedAt = DateTimeOffset.UtcNow
                };

                await _paymentRepo.AddAsync(payment);
            }
            else
            {
                payment.Method = PaymentMethod.Cash;
                payment.Status = PaymentStatus.Completed;
                payment.ProcessedAt = DateTimeOffset.UtcNow;

                _paymentRepo.Update(payment);
            }

            await _paymentRepo.SaveChangesAsync();
            return true;
        }

        public async Task<PaymentDto> CreatePaymentRecordAsync(Guid tripId, decimal amount, PaymentMethod method)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending,
                ProcessedAt = DateTimeOffset.UtcNow
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            return new PaymentDto
            {
                Id = payment.Id,
                RideId = payment.TripId,
                Amount = payment.Amount,
                PaymentMethod = payment.Method,
                Status = (Status)(int)payment.Status,
                TransactionId = payment.GatewayTransactionId,
                CreatedAt = payment.ProcessedAt
            };
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync(Guid passengerId)
        {
            var spec = new PaymentByPassengerSpecification(passengerId);
            var payments = await _paymentRepo.GetAllWithSpecAsync(spec);

            if (payments == null || !payments.Any())
                return Enumerable.Empty<PaymentDto>();

            return payments.Select(payment => new PaymentDto
            {
                Id = payment.Id,
                RideId = payment.TripId,
                Amount = payment.Amount,
                PaymentMethod = payment.Method,
                Status = (Status)(int)payment.Status,
                TransactionId = payment.GatewayTransactionId,
                CreatedAt = payment.ProcessedAt
            }).ToList();
        }

        public async Task<PaymentDto?> GetPaymentStatusAsync(Guid tripId)
        {
            var payment = await _paymentRepo.GetByAsync(p => p.TripId == tripId);

            if (payment == null)
                return null;

            return new PaymentDto
            {
                Id = payment.Id,
                RideId = payment.TripId,
                Amount = payment.Amount,
                PaymentMethod = payment.Method,
                Status = (Status)(int)payment.Status,
                TransactionId = payment.GatewayTransactionId,
                CreatedAt = payment.ProcessedAt
            };
        }

        public async Task<string> InitiatePaymobPaymentAsync(Guid tripId, decimal amount, string passengerEmail, string passengerPhone)
        {
            var apiKey = _configuration["Paymob:ApiKey"];
            var integrationId = int.Parse(_configuration["Paymob:IntegrationId"] ?? "0");
            var iframeId = _configuration["Paymob:IframeId"];

            var authResponse = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", new { api_key = apiKey });
            if (!authResponse.IsSuccessStatusCode)
                throw new Exception("Failed to authenticate with Paymob.");

            var authData = await authResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            string authToken = authData?["token"]?.ToString() ?? throw new Exception("Paymob token is missing.");

            int amountCents = (int)(amount * 100);

            var orderResponse = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", new
            {
                auth_token = authToken,
                delivery_needed = "false",
                amount_cents = amountCents.ToString(),
                currency = "EGP",
                items = new object[] { }
            });

            if (!orderResponse.IsSuccessStatusCode)
                throw new Exception("Failed to create order on Paymob.");

            var orderData = await orderResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var orderId = orderData?["id"]?.ToString() ?? throw new Exception("Paymob Order ID is missing.");

            var paymentKeyResponse = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", new
            {
                auth_token = authToken,
                amount_cents = amountCents.ToString(),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    apartment = "NA",
                    email = passengerEmail,
                    floor = "NA",
                    first_name = "Passenger",
                    street = "NA",
                    building = "NA",
                    phone_number = passengerPhone,
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "EG",
                    last_name = "User",
                    state = "NA"
                },
                currency = "EGP",
                integration_id = integrationId
            });

            if (!paymentKeyResponse.IsSuccessStatusCode)
                throw new Exception("Failed to generate Paymob payment key.");

            var paymentKeyData = await paymentKeyResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            string paymentToken = paymentKeyData?["token"]?.ToString() ?? throw new Exception("Paymob payment token is missing.");

            // تخزين الـ GatewayOrderId الخاص بـ Paymob في السجل
            var existingPayment = await _paymentRepo.GetByAsync(p => p.TripId == tripId);
            if (existingPayment != null)
            {
                existingPayment.GatewayOrderId = orderId;
                _paymentRepo.Update(existingPayment);
                await _paymentRepo.SaveChangesAsync();
            }

            string paymentUrl = $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentToken}";
            return paymentUrl;
        }

        public async Task<bool> ProcessPaymobWebhookAsync(string callbackData)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(callbackData);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("obj", out var objElement))
                    return false;

                bool isSuccess = objElement.GetProperty("success").GetBoolean();
                bool isPending = objElement.GetProperty("pending").GetBoolean();
                long amountCents = objElement.GetProperty("amount_cents").GetInt64();
                string currency = objElement.GetProperty("currency").GetString() ?? "EGP";
                int integrationId = objElement.GetProperty("integration_id").GetInt32();

                // 1. التقاط الـ Transaction ID الحقيقي من معاملة الدفع
                string transactionId = objElement.GetProperty("id").GetInt64().ToString();

                // 2. التقاط الـ Order ID المرتبط بهذه المعاملة
                string orderId = string.Empty;
                if (objElement.TryGetProperty("order", out var orderElement) && orderElement.TryGetProperty("id", out var orderIdElement))
                {
                    orderId = orderIdElement.GetInt64().ToString();
                }

                if (string.IsNullOrEmpty(orderId))
                    return false;

                // 3. البحث في قاعدة البيانات باستخدام الـ GatewayOrderId
                var payment = await _paymentRepo.GetByAsync(p => p.GatewayOrderId == orderId);
                if (payment == null)
                    return false;

                // 4. Idempotency Check
                if (payment.Status == PaymentStatus.Completed || payment.Status == PaymentStatus.Failed)
                {
                    return true;
                }

                // 5. Validation Checks
                long expectedAmountCents = (long)(payment.Amount * 100);
                int configuredIntegrationId = int.Parse(_configuration["Paymob:IntegrationId"] ?? "0");

                if (amountCents != expectedAmountCents || currency != "EGP" || integrationId != configuredIntegrationId)
                {
                    return false;
                }

                // 6. تحديث الحالة وحفظ الـ Transaction ID الفعلي
                if (isSuccess && !isPending)
                {
                    payment.Status = PaymentStatus.Completed;
                    payment.GatewayTransactionId = transactionId; // تخزين الـ Transaction ID الحقيقي للـ Refund

                    var trip = await _tripRepo.GetByAsync(t => t.Id == payment.TripId);
                    if (trip != null)
                    {
                        _tripRepo.Update(trip);
                    }
                }
                else if (!isPending)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.GatewayTransactionId = transactionId;
                }
                else
                {
                    return true;
                }

                payment.ProcessedAt = DateTimeOffset.UtcNow;
                _paymentRepo.Update(payment);
                await _paymentRepo.SaveChangesAsync();

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public async Task<bool> RefundPaymentAsync(Guid paymentId)
        {
            var payment = await _paymentRepo.GetByAsync(p => p.Id == paymentId);

            if (payment == null || payment.Status != PaymentStatus.Completed)
                return false;

            // التأكد من توفر الـ GatewayTransactionId الفعلي وليس الـ Order ID
            if (string.IsNullOrEmpty(payment.GatewayTransactionId))
                return false;

            try
            {
                var apiKey = _configuration["Paymob:ApiKey"];

                var authResponse = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", new { api_key = apiKey });
                if (!authResponse.IsSuccessStatusCode)
                    return false;

                var authData = await authResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                string authToken = authData?["token"]?.ToString();
                if (string.IsNullOrEmpty(authToken))
                    return false;

                int amountCents = (int)(payment.Amount * 100);

                // إرسال طلب الاسترجاع باستخدام الـ transaction_id الصحيح
                var refundResponse = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/refunds", new
                {
                    auth_token = authToken,
                    transaction_id = long.Parse(payment.GatewayTransactionId),
                    amount_cents = amountCents
                });

                if (!refundResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                var refundResult = await refundResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                bool isRefundSuccessful = true;

                if (refundResult != null && refundResult.ContainsKey("success"))
                {
                    bool.TryParse(refundResult["success"]?.ToString(), out isRefundSuccessful);
                }

                if (!isRefundSuccessful)
                    return false;

                payment.Status = PaymentStatus.Refunded;
                payment.ProcessedAt = DateTimeOffset.UtcNow;

                _paymentRepo.Update(payment);
                await _paymentRepo.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
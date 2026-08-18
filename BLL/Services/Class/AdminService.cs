using BLL.Error; // عشان يستدعي الـ Custom Exceptions
using BLL.Services.Interface;
using BLL.Specification.Class;
using DAL.DTOs.Auth;
using DAL.DTOs.DriverDTO;
using DAL.Models;
using DAL.Reposetoriy;
using DAL.Specification.Class;
using DAL.UnitOfWork.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class AdminService : IAdminUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<App_User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IUnitOfWork unitOfWork,
            UserManager<App_User> userManager,
            IEmailService emailService,
            IDistributedCache cache,
            IConfiguration configuration,
            ILogger<AdminService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailService = emailService;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
        }

        private async Task<int> GetPendingDriversVersionAsync()
        {
            var versionStr = await _cache.GetStringAsync("pending_drivers_version");
            if (string.IsNullOrEmpty(versionStr) || !int.TryParse(versionStr, out int version))
            {
                version = 1;
                await _cache.SetStringAsync("pending_drivers_version", version.ToString());
            }
            return version;
        }

        private async Task IncrementPendingDriversVersionAsync()
        {
            var version = await GetPendingDriversVersionAsync();
            version++;
            await _cache.SetStringAsync("pending_drivers_version", version.ToString());
        }

        public async Task<bool> ApproveDriverAsync(Guid driverId)
        {
            var spec = new DriverDocumentSpecification(driverId);
            var driverDoc = await _unitOfWork.GetRepository<DriverDocument>().GetByIdWithSpecAsync(spec);

            if (driverDoc == null)
                throw new NotFoundException("Driver document not found.");

            if (driverDoc.Status == Driveracceptstatus.Approved)
                throw new BadRequestException("Driver is already approved.");

            driverDoc.Status = Driveracceptstatus.Approved;
            driverDoc.RejectionReason = null;

            if (driverDoc.Driver != null)
            {
                driverDoc.Driver.VerifiedAt = DateTimeOffset.UtcNow;
                _unitOfWork.GetRepository<Driver>().Update(driverDoc.Driver);
            }

            _unitOfWork.GetRepository<DriverDocument>().Update(driverDoc);
            await _unitOfWork.CompleteAsync();
            var appUser = driverDoc.Driver?.User;
            if (appUser != null && !await _userManager.IsEmailConfirmedAsync(appUser))
            {
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

                    var baseUrl = _configuration["MailSettings:BaseUrl"] ?? _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7020";
                    var confirmationUrl = $"{baseUrl}/api/auth/confirm-email?userId={appUser.Id}&token={Uri.EscapeDataString(token)}";

                    await _emailService.SendEmailAsync(appUser.Email!, confirmationUrl, "Account Approved - Confirm Your Email");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send approval email to driver {DriverId}", driverId);
                }
            }

            await IncrementPendingDriversVersionAsync();
            return true;
        }

        public async Task RejectDriverAsync(Guid driverId, string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new BadRequestException("Rejection reason is required.");

            var spec = new DriverDocumentSpecification(driverId);
            var driverDoc = await _unitOfWork.GetRepository<DriverDocument>().GetByIdWithSpecAsync(spec);

            if (driverDoc == null)
                throw new NotFoundException("Driver document not found.");

            if (driverDoc.Status == Driveracceptstatus.Rejected)
                throw new BadRequestException("Driver is already rejected.");

            driverDoc.Status = Driveracceptstatus.Rejected;
            driverDoc.RejectionReason = rejectionReason;

            if (driverDoc.Driver != null)
            {
                driverDoc.Driver.VerifiedAt = null;
                _unitOfWork.GetRepository<Driver>().Update(driverDoc.Driver);
            }

            _unitOfWork.GetRepository<DriverDocument>().Update(driverDoc);
            await _unitOfWork.CompleteAsync();

            var appUser = driverDoc.Driver?.User;
            if (appUser != null && !string.IsNullOrEmpty(appUser.Email))
            {
                try
                {
                    // بناء رسالة أو محتوى الرفض وإرساله بالطريقة المباشرة
                    string rejectionMessage = $"Your driver application has been rejected. Reason: {rejectionReason}";

                    await _emailService.SendEmailAsync(appUser.Email, rejectionMessage, "Driver Application Rejected");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send rejection email to driver {DriverId}", driverId);
                }
            }
            await IncrementPendingDriversVersionAsync();
        }

        public async Task<DriverDetailsDto> GetDriverDetailsForAdminAsync(Guid driverId)
        {
            var spec = new DriverDocumentSpecification(driverId);
            var driverDoc = await _unitOfWork.GetRepository<DriverDocument>().GetByIdWithSpecAsync(spec);

            if (driverDoc == null)
                throw new NotFoundException("Driver document not found.");

            return new DriverDetailsDto
            {
                DriverId = driverDoc.DriverId,
                UserName = driverDoc.Driver?.Name,
                Email = driverDoc.Driver?.Email,
                PhoneNumber = driverDoc.Driver?.PhoneNumber,
                Status = driverDoc.Status.ToString(),
                RejectionReason = driverDoc.RejectionReason,
                LicenseNumber = driverDoc.LicenseNumber,
                CarModel = driverDoc.CarModel,
                CarPlateNumber = driverDoc.CarPlateNumber,
                CarCategory = driverDoc.CarCategory.ToString(),
                NationalIdImageUrl = driverDoc.NationalIdImageUrl,
                DriverLicenseImageUrl = driverDoc.DriverLicenseImageUrl,
                CarLicenseImageUrl = driverDoc.CarLicenseImageUrl
            };
        }

        public async Task<IEnumerable<PendingDriverDto>> GetPendingDriversQueueAsync(int pageNumber = 1, int pageSize = 10)
        {
            int version = await GetPendingDriversVersionAsync();
            string cacheKey = $"pending_drivers_v{version}_page_{Math.Max(1, pageNumber)}_size_{Math.Clamp(pageSize, 1, 100)}";

            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData)) return JsonSerializer.Deserialize<IEnumerable<PendingDriverDto>>(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache error for key: {CacheKey}", cacheKey);
            }

            var spec = new PendingDriversSpecification(pageNumber, pageSize);
            var driverDocs = await _unitOfWork.GetRepository<DriverDocument>().GetAllWithSpecAsync(spec);

            var result = driverDocs.Select(d => new PendingDriverDto
            {
                DriverId = d.DriverId,
                Email = d.Driver?.Email,
                FullName = d.Driver?.Name,
                PhoneNumber = d.Driver?.PhoneNumber,
            }).ToList();

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return result;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException("User not found.");

            return new UserDto { Id = user.Id, FirstName = user.UserName, Email = user.Email };
        }

        public async Task<IEnumerable<UserDto>> ListAllUsersAsync(UserQueryParameters queryParams)
        {
            var spec = new UserSpecification(queryParams);
            var data = await _unitOfWork.GetRepository<App_User>().GetAllWithSpecAsync(spec);
            return data.Select(u => new UserDto { Id = u.Id, FirstName = u.UserName, Email = u.Email });
        }

        public async Task SuspendUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException("User not found.");

            user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(30);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException($"Failed to suspend user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        public async Task UnBlockUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException("User not found.");

            user.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException($"Failed to unblock user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
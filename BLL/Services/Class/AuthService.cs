using BLL.Helper;
using BLL.Services.Interface;
using DAL.DTOs.Auth;
using DAL.DTOs.DriverDTO;
using DAL.Models;
using DAL.Specification.Class;
using DAL.UnitOfWork.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<App_User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly Jwt _jwtSettings;
        private readonly string _baseUrl;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<App_User> userManager,
            RoleManager<IdentityRole<Guid>>  roleManager,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IOptions<Jwt> jwtSettings,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
            _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://localhost:7020";
            _logger = logger;
        }

        public async Task<AuthResult> RegisterPassengerAsync(RegisterPassengerDto model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return new AuthResult { Succeeded = false, Message = "Email is already registered." };

            var user = new App_User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}",
                Role = UserRole.Passenger,
                CreatedAt = DateTimeOffset.UtcNow
            };

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new AuthResult { Succeeded = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };
                }

                if (!await _roleManager.RoleExistsAsync(Roles.Passenger))
                {
                    var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Passenger));
                    if (!roleCreateResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return new AuthResult { Succeeded = false, Message = "Failed to create passenger role." };
                    }
                }

                var roleResult = await _userManager.AddToRoleAsync(user, Roles.Passenger);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new AuthResult { Succeeded = false, Message = "Failed to assign user role." };
                }

                await transaction.CommitAsync();

                await SendConfirmationEmailInternalAsync(user);

                return new AuthResult
                {
                    Succeeded = true,
                    Message = "Registration completed successfully. Please check your email to confirm your account."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred during passenger registration for {Email}", model.Email);
                throw;
            }
        }

        public async Task<AuthResult> RegisterDriverAsync(RegisterDriverDto model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return new AuthResult { Succeeded = false, Message = "Email is already registered." };

            var user = new App_User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}",
                Role = UserRole.Driver,
                CreatedAt = DateTimeOffset.UtcNow
            };

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new AuthResult { Succeeded = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };
                }

                if (!await _roleManager.RoleExistsAsync(Roles.Driver))
                {
                    var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Driver));
                    if (!roleCreateResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return new AuthResult { Succeeded = false, Message = "Failed to create driver role." };
                    }
                }

                var roleResult = await _userManager.AddToRoleAsync(user, Roles.Driver);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new AuthResult { Succeeded = false, Message = "Failed to assign driver role." };
                }

                var driverDocument = new DriverDocument
                {
                    DriverId = user.Id,
                    LicenseNumber = model.LicenseNumber,
                    CarModel = model.CarModel,
                    CarPlateNumber = model.CarPlateNumber,
                    CarCategory = model.CarCategory,
                    NationalIdImageUrl = model.NationalIdImageUrl,
                    DriverLicenseImageUrl = model.DriverLicenseImageUrl,
                    CarLicenseImageUrl = model.CarLicenseImageUrl,
                    Status = Driveracceptstatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.GetRepository<DriverDocument>().AddAsync(driverDocument);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                return new AuthResult
                {
                    Succeeded = true,
                    Message = "Driver registration submitted successfully. Your account is pending admin approval."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred during driver registration for {Email}", model.Email);
                throw;
            }
        }

        public async Task<AuthResult> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthResult { Succeeded = false, Message = "Invalid email or password." };

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return new AuthResult { Succeeded = false, Message = "Please confirm your email first." };

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.Driver))
            {
                var driverDoc = await _unitOfWork.GetRepository<DriverDocument>()
                    .GetByIdWithSpecAsync(new DriverDocumentSpecification(user.Id));

                if (driverDoc == null || driverDoc.Status == Driveracceptstatus.Pending)
                    return new AuthResult { Succeeded = false, Message = "Your account is still pending admin approval." };

                if (driverDoc.Status == Driveracceptstatus.Rejected)
                {
                    var reason = string.IsNullOrWhiteSpace(driverDoc.RejectionReason) ? "No reason was provided." : driverDoc.RejectionReason;
                    return new AuthResult { Succeeded = false, Message = $"Your account has been rejected. Reason: {reason}" };
                }
            }

            var jwtToken = await CreateJwtTokenAsync(user);
            var rawRefreshToken = GenerateRefreshToken();

            user.RefreshToken = HashToken(rawRefreshToken);
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return new AuthResult { Succeeded = false, Message = "Failed to process authentication data." };

            return new AuthResult
            {
                Succeeded = true,
                Message = "Login Success",
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                ExpiresOn = jwtToken.ValidTo,
                RefreshToken = rawRefreshToken,
                RefreshTokenExpiration = user.RefreshTokenExpireTime,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            var hashedToken = HashToken(refreshToken);

            var user = await _userManager.Users
                .SingleOrDefaultAsync(u => u.RefreshToken == hashedToken);

            if (user == null || user.RefreshTokenExpireTime <= DateTime.UtcNow)
                return new AuthResult { Succeeded = false, Message = "Invalid or expired refresh token." };

            var jwtToken = await CreateJwtTokenAsync(user);
            var newRawRefreshToken = GenerateRefreshToken();

            user.RefreshToken = HashToken(newRawRefreshToken);
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return new AuthResult { Succeeded = false, Message = "Failed to refresh token." };

            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResult
            {
                Succeeded = true,
                Message = "Token refreshed successfully",
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                ExpiresOn = jwtToken.ValidTo,
                RefreshToken = newRawRefreshToken,
                RefreshTokenExpiration = user.RefreshTokenExpireTime,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }

        public async Task<bool> RevokeTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpireTime = default;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<AuthResult> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = $"{_baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

                try
                {
                    await _emailService.SendEmailAsync(user.Email!, resetLink, "auth", resetLink, "Reset Password - RideShere");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                }
            }

            return new AuthResult
            {
                Succeeded = true,
                Message = "If the account exists, a password reset link has been sent to your email."
            };
        }

        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new AuthResult { Succeeded = false, Message = "Invalid password reset request." };

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
                return new AuthResult { Succeeded = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            user.RefreshToken = null;
            user.RefreshTokenExpireTime = default;
            await _userManager.UpdateAsync(user);

            return new AuthResult { Succeeded = true, Message = "Password has been reset successfully." };
        }

        public async Task<AuthResult> ConfirmEmailAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
                return new AuthResult { Succeeded = false, Message = "Invalid user ID or token." };

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new AuthResult { Succeeded = false, Message = "User not found." };

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return new AuthResult { Succeeded = false, Message = "Email confirmation failed." };

            return new AuthResult { Succeeded = true, Message = "Email confirmed successfully. You can now login." };
        }

        public async Task<AuthResult> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                // منع User Enumeration (نفس رسالة النجاح حتى لو المستخدم غير موجود أو مؤكد مسبقاً)
                return new AuthResult
                {
                    Succeeded = true,
                    Message = "If your email is registered and unconfirmed, a confirmation link has been sent."
                };
            }

            await SendConfirmationEmailInternalAsync(user);

            return new AuthResult
            {
                Succeeded = true,
                Message = "If your email is registered and unconfirmed, a confirmation link has been sent."
            };
        }

        private async Task SendConfirmationEmailInternalAsync(App_User user)
        {
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"{_baseUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await _emailService.SendEmailAsync(user.Email!, confirmationLink, "auth", confirmationLink, "Confirm Your Email - RideShere");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
            }
        }

        private async Task<JwtSecurityToken> CreateJwtTokenAsync(App_User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
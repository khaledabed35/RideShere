using DAL.DTOs.Auth;
using DAL.DTOs.DriverDTO;
using System;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterPassengerAsync(RegisterPassengerDto model);
        Task<AuthResult> RegisterDriverAsync(RegisterDriverDto model);
        Task<AuthResult> LoginAsync(LoginDto model);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string email);
        Task<AuthResult> ForgotPasswordAsync(string email);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordDto model);
        Task<AuthResult> ConfirmEmailAsync(Guid userId, string token);
        Task<AuthResult> ResendConfirmationEmailAsync(string email);
    }
}
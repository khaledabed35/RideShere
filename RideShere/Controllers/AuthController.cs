using BLL.Services.Interface;
using DAL.DTOs.Auth;
using DAL.DTOs.DriverDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IEmailService emailService, IAuthService authService)
        {
            _emailService = emailService;
            _authService = authService;
        }

        [HttpGet("test-mail")]
        public async Task<IActionResult> Test()
        {
            var result = await _emailService.SendSimpleEmailAsync(
                "abedk9072@gmail.com",
                "Test",
                "<h1>Hello</h1>");

            return Ok(result);
        }

        [HttpPost("register-passenger")]
        public async Task<IActionResult> RegisterPassenger([FromBody] RegisterPassengerDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterPassengerAsync(model);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register-driver")]
        public async Task<IActionResult> RegisterDriver([FromBody] RegisterDriverDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterDriverAsync(model);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);

            if (!result.Succeeded)
                return Unauthorized(result);

            if (!string.IsNullOrEmpty(result.Token))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Path = "/",
                    SameSite = SameSiteMode.None,
                    Expires = result.ExpiresOn
                };
                Response.Cookies.Append("jwtToken", result.Token, cookieOptions);
            }

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(dto.RefreshToken))
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

            if (!result.Succeeded)
                return BadRequest(result);

            if (!string.IsNullOrEmpty(result.Token))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Path = "/",
                    SameSite = SameSiteMode.None,
                    Expires = result.ExpiresOn
                };
                Response.Cookies.Append("jwtToken", result.Token, cookieOptions);
            }

            return Ok(result);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto model)
        {
            var email = model?.Email ?? User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { Message = "Email is required." });

            var success = await _authService.RevokeTokenAsync(email);
            if (!success)
                return BadRequest(new { Message = "Failed to revoke token." });

            return Ok(new { Message = "Token revoked successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(model.Email);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(model);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
                return BadRequest(new { Message = "UserId and Token are required." });

            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailDto model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(ModelState);

            var result = await _authService.ResendConfirmationEmailAsync(model.Email);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                await _authService.RevokeTokenAsync(email);
            }

            Response.Cookies.Delete("jwtToken");
            return Ok(new { Message = "Logout successful!" });
        }
    }
}
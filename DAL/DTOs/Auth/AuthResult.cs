using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.DTOs.Auth
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
        public DateTime? ExpiresOn { get; set; }
    }
    public class ResendEmailDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
    }
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
    }
    public class RevokeTokenDto
    {
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}

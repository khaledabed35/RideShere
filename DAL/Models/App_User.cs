using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public enum UserRole
    {
        Passenger,
        Driver,
        Admin
    }

    public class App_User : IdentityUser<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;


        [Required]
        public UserRole Role { get; set; } = UserRole.Passenger; 

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpireTime { get; set; }
        public ICollection<AddressModel> Addresses { get; set; } = new List<AddressModel>();
        public ICollection<Notification> notifications { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using DAL.Models;

namespace DAL.Data.AuthModel
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty; // متوافق مع App_User

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Passenger; // الاستعانة بالـ Enum
    }
}
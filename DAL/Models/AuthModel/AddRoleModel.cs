using System.ComponentModel.DataAnnotations;
using DAL.Models;

namespace DAL.Data.AuthModel
{
    public class AddRoleModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } 
    }
}
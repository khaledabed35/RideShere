using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.DriverDTO
{
    public class RegisterDriverDto
    {
        [Required(ErrorMessage = "Email is required")]

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;



        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = null!;

        // بيانات السائق والرخصة
        [Required(ErrorMessage = "License number is required")]
        [StringLength(50)]
        public string LicenseNumber { get; set; } = null!;

        // بيانات السيارة
        [Required(ErrorMessage = "Car model is required")]
        [StringLength(50)]
        public string CarModel { get; set; } = null!;

        [Required(ErrorMessage = "Car plate number is required")]
        [StringLength(20)]
        public string CarPlateNumber { get; set; } = null!;

        [Required(ErrorMessage = "Car category is required (e.g. Economy, Luxury)")]
        [StringLength(30)]
        public string CarCategory { get; set; } = null!;

        // صور المستندات المطلوبة للتسجيل (روابط أو مسارات الملفات المرفوعة)
        [Required(ErrorMessage = "National ID image is required")]
        public string NationalIdImageUrl { get; set; } = null!;

        [Required(ErrorMessage = "Driver license image is required")]
        public string DriverLicenseImageUrl { get; set; } = null!;

        [Required(ErrorMessage = "Car license image is required")]
        public string CarLicenseImageUrl { get; set; } = null!;
    }
}
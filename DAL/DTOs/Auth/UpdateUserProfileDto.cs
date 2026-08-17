using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.Auth
{
    public class UpdateUserProfileDto
    {
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string FullName { get; set; }
        public int Address { get; set; }
    }
}

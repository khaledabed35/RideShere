using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.Auth
{

    
        public class UserprofileDto
        {
            public string Id { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public bool IsEmailConfirmed { get; set; }
            public string? ProfilePictureUrl { get; set; }
        }
   }



using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.Auth
{
    public class TokenRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

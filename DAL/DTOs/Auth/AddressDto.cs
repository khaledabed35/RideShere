using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.Auth
{
   
        public class AddressDto
        {
            public string AddressName { get; set; } = string.Empty; // مثل: "Home", "Work"
            public string City { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;     // التفاصيل (الشارع، العمارة)
        }
    }


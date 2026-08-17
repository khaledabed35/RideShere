using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.Auth
{
    public class FavoritePlaceDto
    {
        public string AddressName { get; set; } = string.Empty; 
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;     
        public string PhoneNumber { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs.Auth
{
    public class AdminDto


    {
        public Guid id { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string username { get; set; }
        public string fullname { get; set; }
    }
}

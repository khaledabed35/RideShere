using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Helper
{
    public class Jwt
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }

        public int DurationInDays { get; set; }

        public int DurationInMinutes => DurationInDays * 24 * 60;
    }
}

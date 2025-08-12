using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Auth
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = "IKARUSWEB";
        public string Audience { get; set; } = "IKARUSWEB_API";
        public string Key { get; set; } = "";  // dev için appsettings, prod için KeyVault
        public int ExpiryMinutes { get; set; } = 60;
    }
}

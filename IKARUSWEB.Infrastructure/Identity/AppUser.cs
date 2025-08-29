using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        public Guid? TenantId { get; set; }
        public string? TenantCode { get; set; }
        public string? FullName { get; set; }
    }
}

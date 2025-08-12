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
        public Guid? TenantId { get; set; }           // Kullanıcının oteli/tenant'ı
        public string? FullName { get; set; }
    }
}

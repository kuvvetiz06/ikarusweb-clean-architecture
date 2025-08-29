using IKARUSWEB.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Identity
{
    public sealed class CurrentTenant : ITenantProvider
    {
        public Guid? TenantId { get; }
        public bool IsSuperUser { get; } = false; // ihtiyaca göre role-check eklenebilir

        public CurrentTenant(IHttpContextAccessor http)
        {
            var tid = http.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            TenantId = Guid.TryParse(tid, out var g) ? g : null;
        }
    }
}

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
        public Guid TenantId { get; }
        public bool IsSuperUser { get; } = false; // ihtiyaca göre role-check eklenebilir

        public bool IsResolved { get; }   // ← ek

        public CurrentTenant(IHttpContextAccessor http)
        {
            var tid = http.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            //TenantId = Guid.TryParse(tid, out var g) ? g : null;

            if (Guid.TryParse(tid, out var id))
            {
                TenantId = id;
                IsResolved = true;
            }
            else
            {
                TenantId = Guid.Empty;   // login öncesi/anonim istekler
                IsResolved = false;
            }
        }
    }

    public sealed class TenantNotResolvedException : Exception
    {
        public TenantNotResolvedException() : base("Tenant not resolved.") { }
    }
}

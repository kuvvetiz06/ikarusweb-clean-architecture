using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Abstractions
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        string? UserName { get; }
        Guid? TenantId { get; } // JWT’de "tenantId" claim’inden okunacak

        public string? TenantName { get; }
    }
}

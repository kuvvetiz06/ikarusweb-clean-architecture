using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Common.Security
{
    public sealed record UserTicket(
    Guid UserId,
    Guid? TenantId,
    string UserName,
    IList<string>? Roles,
    string? TenantName = null,
    string? FullName = null
);
}

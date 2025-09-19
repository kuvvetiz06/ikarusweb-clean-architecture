using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Tenants.Dtos
{
    public sealed record TenantDto(
       Guid Id,
       string Name,
       string Code,
       string Street,
       string City,
       string Country,
       string? DefaultCurrencyCode,
       string TimeZone,
       string DefaultCulture
   );
}

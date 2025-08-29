using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Mapping
{
    public sealed record TenantDto(
       Guid Id,
       string Name,
       string Code,
       string Street,
       string City,
       string Country,
       string DefaultCurrency,
       string TimeZone,
       string DefaultCulture
   );
}

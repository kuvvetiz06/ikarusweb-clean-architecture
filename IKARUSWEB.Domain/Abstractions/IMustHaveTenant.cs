using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Abstractions
{
    public interface IMustHaveTenant
    {
        Guid TenantId { get; }
    }
}

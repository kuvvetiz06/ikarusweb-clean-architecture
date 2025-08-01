using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities.Common
{
    public abstract class MultiTenantEntity : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
    }
}

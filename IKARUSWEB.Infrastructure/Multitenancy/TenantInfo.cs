using Finbuckle.MultiTenant.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Multitenancy
{
    public class TenantInfo : ITenantInfo
    {
        public string Id { get; set; } = null!;
        public string Identifier { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
    }
}

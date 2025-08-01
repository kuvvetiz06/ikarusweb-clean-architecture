using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using IKARUSWEB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Persistence
{
    public class IKARUSWEBDbContext : MultiTenantDbContext
    {
        public IKARUSWEBDbContext(
            IMultiTenantContextAccessor tenantAccessor,
            DbContextOptions<IKARUSWEBDbContext> options)
            : base(tenantAccessor, options)
        {
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<RoomType> RoomTypes => Set<RoomType>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new Configurations.TenantConfiguration());
            // Diğer entity konfigürasyonları...
        }
    }
}

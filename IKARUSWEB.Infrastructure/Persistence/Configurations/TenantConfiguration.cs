using IKARUSWEB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Persistence.Configurations
{
    public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> b)
        {
            b.ToTable("Tenants");

            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Street).IsRequired().HasMaxLength(200);
            b.Property(x => x.City).IsRequired().HasMaxLength(100);
            b.Property(x => x.Country).IsRequired().HasMaxLength(100);
            b.Property(x => x.DefaultCurrency).IsRequired().HasMaxLength(3);
            b.Property(x => x.TimeZone).IsRequired().HasMaxLength(100);
            b.Property(x => x.DefaultCulture).IsRequired().HasMaxLength(10);

            b.HasIndex(x => x.Name).IsUnique();

            // Soft-delete flag'leri için filtre KULLANMIYORUZ (sorgu seviyesinde karar vereceğiz).
            b.HasMany(x => x.Rooms)
             .WithOne(x => x.Tenant)
             .HasForeignKey(x => x.TenantId)
             .OnDelete(DeleteBehavior.Restrict); // soft-delete tercih ettiğimiz için cascade kapalı
        }
    }
}

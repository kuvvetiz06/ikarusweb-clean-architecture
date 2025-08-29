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
            b.HasIndex(x => x.Name).IsUnique();

            b.Property(x => x.Code).IsRequired().HasMaxLength(6);
            b.Property(x => x.Country).IsRequired().HasMaxLength(100);
            b.Property(x => x.City).IsRequired().HasMaxLength(100);
            b.Property(x => x.Street).IsRequired().HasMaxLength(200);

            b.Property(x => x.DefaultCurrencyCode).HasMaxLength(3).IsUnicode(false); ;
            b.HasCheckConstraint("CK_Tenant_DefaultCurrencyCode_NullOrLen3", "[DefaultCurrencyCode] IS NULL OR LEN([DefaultCurrencyCode]) = 3");

            b.Property(x => x.TimeZone).IsRequired().HasMaxLength(100);
            b.Property(x => x.DefaultCulture).IsRequired().HasMaxLength(10);

            // Tenant(Id, DefaultCurrencyCode) --> Currency(TenantId, Code)s
            // 1) Dependent (Tenant) FK: SIRAYA DİKKAT: Id, DefaultCurrencyCode
            // 2) Principal (Currency) AK:  TenantId, Code
            b.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => new { x.Id, x.DefaultCurrencyCode })
            .HasPrincipalKey(c => new { c.TenantId, c.Code })
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}

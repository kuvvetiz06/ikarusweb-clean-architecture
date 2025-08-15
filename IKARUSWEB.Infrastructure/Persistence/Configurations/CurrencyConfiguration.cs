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
    public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> b)
        {
            b.ToTable("Currencies");
            b.HasKey(x => x.Id);

            b.Property(x => x.TenantId).IsRequired();

            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Code).IsRequired().IsUnicode(false).HasMaxLength(3);

            b.Property(x => x.CurrencyMultiplier).IsRequired().HasPrecision(18, 6);
            b.Property(x => x.Rate).IsRequired().HasPrecision(18, 6);

            // Tenant içinde benzersizlikler
            b.HasAlternateKey(x => new { x.TenantId, x.Code });         // principal key (Tenant FK için)
            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();

            // Check constraints
            b.HasCheckConstraint("CK_Currency_Code_Len3", "LEN([Code]) = 3");
            b.HasCheckConstraint("CK_Currency_Multiplier_Positive", "[CurrencyMultiplier] > 0");
            b.HasCheckConstraint("CK_Currency_Rate_NonNegative", "[Rate] >= 0");

            // Tenant FK
            b.HasOne<Tenant>()
             .WithMany()
             .HasForeignKey(x => x.TenantId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

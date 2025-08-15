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
    public sealed class RoomViewConfiguration : IEntityTypeConfiguration<RoomView>
    {
        public void Configure(EntityTypeBuilder<RoomView> b)
        {
            b.ToTable("RoomViews");
            b.HasKey(x => x.Id);

            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Code).HasMaxLength(20);
            b.Property(x => x.Description).HasMaxLength(500);

            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[Code] IS NOT NULL");

            b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}

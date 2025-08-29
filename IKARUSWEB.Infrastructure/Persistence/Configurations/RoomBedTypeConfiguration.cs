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
    public sealed class RoomBedTypeConfiguration : IEntityTypeConfiguration<RoomBedType>
    {        
        public void Configure(EntityTypeBuilder<RoomBedType> b)
        {
            b.ToTable("RoomBedTypes");
            b.HasKey(x => x.Id);

            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Code).HasMaxLength(32);
            b.Property(x => x.Description).HasMaxLength(500);

            b.HasQueryFilter(x => !x.IsDeleted);

            
            b.HasIndex(x => new { x.TenantId, x.Name })
             .IsUnique()
             .HasFilter("[IsDeleted] = 0"); // SQL Server

            b.HasIndex(x => new { x.TenantId, x.Code })
             .IsUnique()
             .HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
        }
    }
}

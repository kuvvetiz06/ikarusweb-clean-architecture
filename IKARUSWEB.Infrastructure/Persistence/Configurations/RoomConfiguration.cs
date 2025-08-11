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

    public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> b)
        {
            b.ToTable("Rooms");
            b.HasKey(x => x.Id);

            b.Property(x => x.Number).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.Floor).HasMaxLength(20);
            b.Property(x => x.MaxBed).IsRequired();

            // Tenant zorunlu + oda numarası tenant bazında unique
            b.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();

            b.HasOne(x => x.Tenant)
                .WithMany(t => t.Rooms)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.RoomType)
                .WithMany()
                .HasForeignKey(x => x.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.RoomView)
                .WithMany()
                .HasForeignKey(x => x.RoomViewId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.RoomLocation)
                .WithMany()
                .HasForeignKey(x => x.RoomLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.RoomBedType)
                .WithMany()
                .HasForeignKey(x => x.RoomBedTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
  
}

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

            b.Property(x => x.TenantId).IsRequired();

            // Detay alanlar
            b.Property(x => x.Number).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.Floor).HasMaxLength(20);
            b.Property(x => x.MaxBed).IsRequired();

            // ≥ 1 kuralı
            b.HasCheckConstraint("CK_Room_MaxBed_Min1", "[MaxBed] >= 1");

            // Kiracı içinde oda numarası benzersiz
            b.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();

            // Zorunlu ilişkiler
            b.HasOne(x => x.RoomType).WithMany(rt => rt.Rooms).HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.RoomView).WithMany(rv => rv.Rooms).HasForeignKey(x => x.RoomViewId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.RoomBedType).WithMany(rbt => rbt.Rooms).HasForeignKey(x => x.RoomBedTypeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.RoomLocation).WithMany(rl => rl.Rooms).HasForeignKey(x => x.RoomLocationId).OnDelete(DeleteBehavior.Restrict);

            // Tenant FK
            b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        }
    }

}

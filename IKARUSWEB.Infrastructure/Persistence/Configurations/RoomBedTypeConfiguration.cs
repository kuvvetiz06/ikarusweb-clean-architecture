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
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Name).IsUnique();
        }
    }
}

using IKARUSWEB.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Persistence.Configurations
{
    public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ToTable("RefreshTokens");
            b.HasKey(x => x.Id);
            b.Property(x => x.Token).IsRequired().HasMaxLength(200);
            b.HasIndex(x => x.Token).IsUnique();
            b.Property(x => x.ExpiresAt).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.TenantId).IsRequired();
        }
    }
}

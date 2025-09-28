using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Domain.Abstractions;
using IKARUSWEB.Domain.Entities;
using IKARUSWEB.Domain.Security;
using IKARUSWEB.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;


namespace IKARUSWEB.Infrastructure.Persistence
{
    public sealed class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>, IAppDbContext
    {

        private readonly ITenantProvider _tenant;
        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenant) : base(options)
        {
            _tenant = tenant; // TenantId ve IsResolved burada
        }

        // EF DbSet'leri
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<RoomType> RoomTypes => Set<RoomType>();
        public DbSet<RoomView> RoomViews => Set<RoomView>();
        public DbSet<RoomBedType> RoomBedTypes => Set<RoomBedType>();
        public DbSet<RoomLocation> RoomLocations => Set<RoomLocation>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>(b =>
            {
                b.Property(u => u.FullName).HasMaxLength(200);
                b.HasOne<Tenant>()
                 .WithMany()
                 .HasForeignKey(u => u.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            ApplyGlobalFilters(modelBuilder);
        }

        private void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            foreach (var et in modelBuilder.Model.GetEntityTypes())
            {
                var clr = et.ClrType;
                if (!typeof(BaseEntity).IsAssignableFrom(clr)) continue;

                // param: e
                var param = Expression.Parameter(clr, "e");

                // soft delete: !((BaseEntity)e).IsDeleted
                var baseProp = Expression.Property(
                Expression.Convert(param, typeof(BaseEntity)),nameof(BaseEntity.IsDeleted));
                Expression body = Expression.Equal(baseProp, Expression.Constant(false));

                // tenant filtresi: yalnızca _tenant.IsResolved == true ise uygula
                if (typeof(IMustHaveTenant).IsAssignableFrom(clr) && _tenant.IsResolved)
                {
                    var tenantProp = Expression.Property(
                    Expression.Convert(param, typeof(IMustHaveTenant)),
                    nameof(IMustHaveTenant.TenantId));

                    var equalsTenant = Expression.Equal(tenantProp,
                    Expression.Constant(_tenant.TenantId)); // Guid, nullable değil

                    body = Expression.AndAlso(body, equalsTenant);
                }

                var lambda = Expression.Lambda(body, param);
                et.SetQueryFilter(lambda);
            }
        }
    }
}

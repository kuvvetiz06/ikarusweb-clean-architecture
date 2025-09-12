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


namespace IKARUSWEB.Infrastructure.Persistence
{
    public sealed class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>, IAppDbContext
    {
     
        private readonly Guid? _currentTenantId;
        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenant) : base(options)
        {
           _currentTenantId = tenant.TenantId; 
      
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

                // e => !((BaseEntity)e).IsDeleted
                var param = Expression.Parameter(clr, "e");
                var baseProp = Expression.Property(Expression.Convert(param, typeof(BaseEntity)), nameof(BaseEntity.IsDeleted));
                var notDeleted = Expression.Equal(baseProp, Expression.Constant(false));

                Expression body = notDeleted;

                // Tenant filtresi: e => ((IMustHaveTenant)e).TenantId == _currentTenantId
                if (typeof(IMustHaveTenant).IsAssignableFrom(clr))
                {
                    var tenantProp = Expression.Property(Expression.Convert(param, typeof(IMustHaveTenant)), nameof(IMustHaveTenant.TenantId));

                    // _currentTenantId yoksa (anon kullanıcı veya sysadmin) filtreyi geç (true)
                    // otherwise: tenant eşitliği
                    var tenantIdConst = Expression.Constant(_currentTenantId, typeof(Guid?));
                    var hasTenant = Expression.NotEqual(tenantIdConst, Expression.Constant(null, typeof(Guid?)));
                    var equals = Expression.Equal(tenantProp, Expression.Convert(tenantIdConst, typeof(Guid)));

                    var tenantClause = Expression.Condition(hasTenant, equals, Expression.Constant(true));
                    body = Expression.AndAlso(body, tenantClause);
                }

                var lambda = Expression.Lambda(body, param);
                et.SetQueryFilter(lambda);
            }
        }
    }
}

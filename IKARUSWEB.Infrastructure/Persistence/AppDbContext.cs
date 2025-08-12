using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Domain.Abstractions;
using IKARUSWEB.Domain.Entities;
using IKARUSWEB.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace IKARUSWEB.Infrastructure.Persistence
{
    public sealed class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>, IAppDbContext
    {
        private readonly IDateTime _clock;

        public AppDbContext(DbContextOptions<AppDbContext> options, IDateTime clock) : base(options)
            => _clock = clock;

        // EF DbSet'leri
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<RoomType> RoomTypes => Set<RoomType>();
        public DbSet<RoomView> RoomViews => Set<RoomView>();
        public DbSet<RoomBedType> RoomBedTypes => Set<RoomBedType>();
        public DbSet<RoomLocation> RoomLocations => Set<RoomLocation>();
                
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
        }
    }
}

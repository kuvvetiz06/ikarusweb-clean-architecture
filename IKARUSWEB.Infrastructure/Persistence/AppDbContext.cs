using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Domain.Abstractions;
using IKARUSWEB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace IKARUSWEB.Infrastructure.Persistence
{
    public sealed class AppDbContext : DbContext, IAppDbContext
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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

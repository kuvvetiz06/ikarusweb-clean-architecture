using IKARUSWEB.Domain.Abstractions;
using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Abstractions
{
    public interface IAppDbContext
    {
        // EF-benzeri set'ler (query/write için)
        IQueryable<Tenant> Tenants { get; }
        IQueryable<Room> Rooms { get; }
        IQueryable<RoomType> RoomTypes { get; }
        IQueryable<RoomView> RoomViews { get; }
        IQueryable<RoomBedType> RoomBedTypes { get; }
        IQueryable<RoomLocation> RoomLocations { get; }

        // Basit CRUD yardımcıları (EF implementasyonu Infrastructure’da)
        Task<T?> FindAsync<T>(Guid id, CancellationToken ct = default) where T : BaseEntity;
        Task AddAsync<T>(T entity, CancellationToken ct = default) where T : BaseEntity;
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

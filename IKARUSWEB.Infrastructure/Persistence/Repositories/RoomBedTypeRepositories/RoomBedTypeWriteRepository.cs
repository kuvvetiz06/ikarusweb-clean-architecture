using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Persistence.Repositories.RoomBedTypeRepositories
{
    public sealed class RoomBedTypeWriteRepository : IRoomBedTypeWriteRepository
    {
        private readonly AppDbContext _db;
        public RoomBedTypeWriteRepository(AppDbContext db) => _db = db;

        public async Task<Guid> CreateAsync(RoomBedType entity, CancellationToken ct = default)
        {
            await _db.Set<RoomBedType>().AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task UpdateAsync(RoomBedType entity, CancellationToken ct = default)
        {
            _db.Set<RoomBedType>().Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(RoomBedType entity, CancellationToken ct = default)
        {
            entity.MarkDeleted();                 
            _db.Set<RoomBedType>().Update(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}

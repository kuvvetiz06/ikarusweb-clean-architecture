using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Persistence.Repositories.RoomBedTypeRepositories
{
    public sealed class RoomBedTypeReadRepository : IRoomBedTypeReadRepository
    {
        private readonly AppDbContext _db;
        private readonly ITenantProvider _tenant;

        public RoomBedTypeReadRepository(AppDbContext db, ITenantProvider tenant)
        { _db = db; _tenant = tenant; }

        public async Task<RoomBedType?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<RoomBedType>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _tenant.TenantId, ct);

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
            => await _db.Set<RoomBedType>().AsNoTracking()
                .AnyAsync(x => x.TenantId == _tenant.TenantId &&
                               x.Name.ToLower() == name.ToLower(), ct);

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
            => await _db.Set<RoomBedType>().AsNoTracking()
                .AnyAsync(x => x.TenantId == _tenant.TenantId &&
                               x.Code != null &&
                               x.Code.ToLower() == code.ToLower(), ct);

        public async Task<List<RoomBedType>> ListAsync(string? q, CancellationToken ct = default)
        {
            var query = _db.Set<RoomBedType>().AsNoTracking()
                .Where(x => x.TenantId == _tenant.TenantId);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x =>
                    x.Name.Contains(q) ||
                    (x.Code != null && x.Code.Contains(q)) ||
                    (x.Description != null && x.Description.Contains(q)));

            return await query.OrderBy(x => x.Name).ToListAsync(ct);
        }
    }
}

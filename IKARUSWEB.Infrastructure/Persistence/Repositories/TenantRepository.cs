using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Persistence.Repositories
{
    public sealed class TenantRepository : ITenantRepository
    {
        private readonly AppDbContext _db;

        public TenantRepository(AppDbContext db) => _db = db;

        public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

        public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
            => _db.Tenants.AsNoTracking().AnyAsync(t => t.Name == name, ct);

        public Task AddAsync(Tenant entity, CancellationToken ct = default)
            => _db.Tenants.AddAsync(entity, ct).AsTask();

        public IQueryable<Tenant> AsQueryable(bool readOnly = true)
            => readOnly ? _db.Tenants.AsNoTracking() : _db.Tenants.AsQueryable();
    }
}

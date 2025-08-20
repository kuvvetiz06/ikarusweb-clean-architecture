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
    public sealed class CurrencyRepository : ICurrencyRepository
    {
        private readonly AppDbContext _db;
        public CurrencyRepository(AppDbContext db) => _db = db;

        public Task<Currency?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.Set<Currency>().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
        {
            var c = code.Trim().ToUpperInvariant();
            return _db.Set<Currency>().AsNoTracking().AnyAsync(x => x.Code == c, ct);
        }

        public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        {
            var n = name.Trim();
            return _db.Set<Currency>().AsNoTracking().AnyAsync(x => x.Name == n, ct);

        }

        public IQueryable<Currency> AsQueryable(bool readOnly = true)
            => readOnly ? _db.Currencies.AsNoTracking() : _db.Currencies.AsQueryable();

        public Task AddAsync(Currency entity, CancellationToken ct = default)
            => _db.Set<Currency>().AddAsync(entity, ct).AsTask();
    }
}

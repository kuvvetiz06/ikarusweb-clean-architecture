using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Abstractions.Repositories
{
    public interface ICurrencyRepository
    {
        // READ
        Task<Currency?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
        IQueryable<Currency> AsQueryable(bool readOnly = true);

        // WRITE
        Task AddAsync(Currency entity, CancellationToken ct = default);
    }
}

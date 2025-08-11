using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Abstractions.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
        Task AddAsync(Tenant entity, CancellationToken ct = default);

        // Projeksiyon/sayfalama için gerekirse:
        IQueryable<Tenant> AsQueryable(bool readOnly = true);
    }
}

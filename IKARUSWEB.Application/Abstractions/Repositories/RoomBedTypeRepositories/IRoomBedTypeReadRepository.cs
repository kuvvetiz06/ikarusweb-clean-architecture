using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories
{
    public interface IRoomBedTypeReadRepository
    {
        Task<RoomBedType?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default); // code optional, null değilse kontrol
        Task<List<RoomBedType>> ListAsync(string? q, CancellationToken ct = default);
    }
}

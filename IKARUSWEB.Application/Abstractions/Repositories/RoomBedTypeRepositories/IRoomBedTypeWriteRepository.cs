using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories
{
    public interface IRoomBedTypeWriteRepository
    {
        Task<Guid> CreateAsync(RoomBedType entity, CancellationToken ct = default);
        Task UpdateAsync(RoomBedType entity, CancellationToken ct = default);
        Task DeleteAsync(RoomBedType entity, CancellationToken ct = default);
    }
}

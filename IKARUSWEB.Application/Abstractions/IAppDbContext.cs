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
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

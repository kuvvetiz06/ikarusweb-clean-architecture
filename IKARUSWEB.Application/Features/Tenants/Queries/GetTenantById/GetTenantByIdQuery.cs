using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Mapping;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Tenants.Queries.GetTenantById
{
    public sealed record GetTenantByIdQuery(Guid Id) : IRequest<Result<TenantDto>>;
}

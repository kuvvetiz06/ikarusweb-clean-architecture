using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Mapping;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Tenants.Commands.CreateTenant
{
    public sealed record CreateTenantCommand(
    string Name,
    string Code,
    string Country,
    string City,
    string Street,
    string DefaultCurrency = "TRY",
    string TimeZone = "Europe/Istanbul",
    string DefaultCulture = "tr-TR"
) : IRequest<Result<TenantDto>>;
}

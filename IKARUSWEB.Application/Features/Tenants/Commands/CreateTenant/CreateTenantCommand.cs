using IKARUSWEB.Application.Features.Tenants.Dtos;
using MediatR;


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

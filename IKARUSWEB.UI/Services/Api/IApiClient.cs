using IKARUSWEB.Application.Mapping;
using IKARUSWEB.UI.Models.Api;
using IKARUSWEB.UI.Models.Currencies;

namespace IKARUSWEB.UI.Services.Api
{
    public interface IApiClient
    {
        Task<string?> LoginAsync(string userName, string password, CancellationToken ct = default);
        Task<TenantVm?> GetTenantAsync(Guid id, CancellationToken ct = default);
        Task<TenantVm?> CreateTenantAsync(CreateTenantDto dto, CancellationToken ct = default);

        Task<IReadOnlyList<CurrencyDto>> GetCurrenciesAsync(string? q, CancellationToken ct = default);
        Task<Result<CurrencyDto>> CreateCurrencyAsync(CreateCurrencyRequest req, CancellationToken ct = default);
        Task<Result<CurrencyDto>> UpdateCurrencyRateAsync(Guid id, decimal rate, CancellationToken ct = default);
    }
}

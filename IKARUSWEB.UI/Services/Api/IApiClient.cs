using IKARUSWEB.UI.Models.Api;

namespace IKARUSWEB.UI.Services.Api
{
    public interface IApiClient
    {
        Task<string?> LoginAsync(string userName, string password, CancellationToken ct = default);
        Task<TenantVm?> GetTenantAsync(Guid id, CancellationToken ct = default);
        Task<TenantVm?> CreateTenantAsync(CreateTenantDto dto, CancellationToken ct = default);
    }
}

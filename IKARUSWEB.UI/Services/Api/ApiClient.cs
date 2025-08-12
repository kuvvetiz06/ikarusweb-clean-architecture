using IKARUSWEB.UI.Models.Api;

namespace IKARUSWEB.UI.Services.Api
{
    public sealed class ApiClient : IApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(HttpClient http) => _http = http;

        public async Task<string?> LoginAsync(string userName, string password, CancellationToken ct = default)
        {
            using var resp = await _http.PostAsJsonAsync("/api/auth/login", new { userName, password }, ct);
            if (!resp.IsSuccessStatusCode) return null;
            var obj = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
            return obj?.access_token;
        }

        public async Task<TenantVm?> GetTenantAsync(Guid id, CancellationToken ct = default)
        {
            using var resp = await _http.GetAsync($"/api/tenants/{id}", ct);
            if (!resp.IsSuccessStatusCode) return null;

            var body = await resp.Content.ReadFromJsonAsync<Result<TenantVm>>(cancellationToken: ct);
            return body is { Succeeded: true, Data: not null } ? body.Data : null;
        }

        public async Task<TenantVm?> CreateTenantAsync(CreateTenantDto dto, CancellationToken ct = default)
        {
            using var resp = await _http.PostAsJsonAsync($"/api/tenants", dto, ct);
            if (!resp.IsSuccessStatusCode) return null;

            var body = await resp.Content.ReadFromJsonAsync<Result<TenantVm>>(cancellationToken: ct);
            return body is { Succeeded: true, Data: not null } ? body.Data : null;
        }

        private sealed record LoginResponse(string access_token);
    }
}

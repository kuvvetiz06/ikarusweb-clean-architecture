using IKARUSWEB.UI.Models.Api;
using IKARUSWEB.UI.Services.Api;
using System.Net;

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    private static async Task ThrowIfError(HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;

        if (resp.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API request unauthorized.");

        ApiErrorEnvelope? env = null;
        try
        {
            env = await resp.Content.ReadFromJsonAsync<ApiErrorEnvelope>(cancellationToken: ct);
        }
        catch { /* body parse edilemezse null kalır */ }

        var msg = env?.Problem?.Detail
                  ?? env?.Problem?.Title
                  ?? resp.ReasonPhrase
                  ?? "Request failed";

        throw new ApiException(env?.Problem, env?.Errors, msg);
    }

    public async Task<string?> LoginAsync(string userName, string password, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsJsonAsync("/api/auth/login", new { userName, password }, ct);
        await ThrowIfError(resp, ct);
        var obj = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        return obj?.access_token;
    }

    public async Task<TenantVm?> GetTenantAsync(Guid id, CancellationToken ct = default)
    {
        using var resp = await _http.GetAsync($"/api/tenants/{id}", ct);
        await ThrowIfError(resp, ct);
        var body = await resp.Content.ReadFromJsonAsync<Result<TenantVm>>(cancellationToken: ct);
        return body?.Data;
    }

    public async Task<TenantVm?> CreateTenantAsync(CreateTenantDto dto, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsJsonAsync($"/api/tenants", dto, ct);
        await ThrowIfError(resp, ct);
        var body = await resp.Content.ReadFromJsonAsync<Result<TenantVm>>(cancellationToken: ct);
        return body?.Data;
    }

    private sealed record LoginResponse(string access_token);
}

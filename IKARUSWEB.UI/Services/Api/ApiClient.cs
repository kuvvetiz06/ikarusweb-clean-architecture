using IKARUSWEB.UI.Models.Api;
using IKARUSWEB.UI.Services.Api;
using System.Net;

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    private sealed class ValidationProblemDto
    {
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
    private static async Task ThrowIfError(HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;

        if (resp.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API request unauthorized.");

        // 1) Bizim zarf (problem + errors [])
        ApiErrorEnvelope? env = null;
        try { env = await resp.Content.ReadFromJsonAsync<ApiErrorEnvelope>(cancellationToken: ct); }
        catch { /* parse edilemedi */ }

        if (env?.Problem is not null || env?.Errors is not null)
            throw new ApiException(env?.Problem, env?.Errors, env?.Problem?.Detail ?? env?.Problem?.Title);

        // 2) MVC’nin ValidationProblemDetails’ı
        ValidationProblemDto? vpd = null;
        try { vpd = await resp.Content.ReadFromJsonAsync<ValidationProblemDto>(cancellationToken: ct); }
        catch { /* parse edilemedi */ }

        if (vpd?.Errors is not null && vpd.Errors.Count > 0)
        {
            var list = vpd.Errors
                .SelectMany(kv => kv.Value.Select(msg => new ApiFieldError { Field = kv.Key, Message = msg }))
                .ToList();
            var pd = new ProblemDetailsDto { Title = vpd.Title, Status = vpd.Status, Detail = vpd.Detail };
            throw new ApiException(pd, list, vpd.Detail ?? vpd.Title ?? "Request failed");
        }

        // 3) Düz ProblemDetails veya metin
        var text = await resp.Content.ReadAsStringAsync(ct);
        throw new ApiException(new ProblemDetailsDto { Title = resp.ReasonPhrase, Status = (int)resp.StatusCode, Detail = text }, null, text);
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

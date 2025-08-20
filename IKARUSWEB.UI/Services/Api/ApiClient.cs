using IKARUSWEB.Application.Mapping;
using IKARUSWEB.UI.Models.Api;
using IKARUSWEB.UI.Models.Currencies;
using IKARUSWEB.UI.Services;
using IKARUSWEB.UI.Services.Api;
using System.Net;
using System.Text;
using System.Text.Json;

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

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

    public async Task<IReadOnlyList<CurrencyDto>> GetCurrenciesAsync(string? q, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(q) ? "api/currencies" : $"api/currencies?q={Uri.EscapeDataString(q)}";
        using var resp = await _http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode)
            throw await HttpApiException.FromResponseAsync(resp);

        var list = await resp.Content.ReadFromJsonAsync<IReadOnlyList<CurrencyDto>>(JsonOpts, ct);
        return list ?? Array.Empty<CurrencyDto>();
    }

    public async Task<Result<CurrencyDto>> CreateCurrencyAsync(CreateCurrencyRequest req, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsJsonAsync("api/currencies", req, JsonOpts, ct);
        if (!resp.IsSuccessStatusCode)
            throw await HttpApiException.FromResponseAsync(resp);

        // API: Result<CurrencyDto> döndürüyor
        var env = await resp.Content.ReadFromJsonAsync<Result<CurrencyDto>>(JsonOpts, ct);
        return env ?? Result<CurrencyDto>.Failure("Empty response");
    }

    public async Task<Result<CurrencyDto>> UpdateCurrencyRateAsync(Guid id, decimal rate, CancellationToken ct = default)
    {
        // Body ham decimal; JSON number olması için serialize edelim
        var content = new StringContent(JsonSerializer.Serialize(rate, JsonOpts), Encoding.UTF8, "application/json");

        using var resp = await _http.PatchAsync($"api/currencies/{id}/rate", content, ct);
        if (!resp.IsSuccessStatusCode)
            throw await HttpApiException.FromResponseAsync(resp);

        var env = await resp.Content.ReadFromJsonAsync<Result<CurrencyDto>>(JsonOpts, ct);
        return env ?? Result<CurrencyDto>.Failure("Empty response");
    }


    private sealed record LoginResponse(string access_token);
}

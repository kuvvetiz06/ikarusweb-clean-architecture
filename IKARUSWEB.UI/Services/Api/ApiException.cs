// IKARUSWEB.UI/Services/Api/ApiException.cs
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IKARUSWEB.UI.Services.Api;

public sealed class ProblemDetailsDto { public string? Title { get; set; } public int? Status { get; set; } public string? Detail { get; set; } }
public sealed class ApiFieldError { public string? Field { get; set; } public string? Code { get; set; } public string? Message { get; set; } }
public sealed class ApiErrorEnvelope { public ProblemDetailsDto? Problem { get; set; } public List<ApiFieldError>? Errors { get; set; } }

public sealed class ApiException : Exception
{
    public ProblemDetailsDto? Problem { get; }
    public List<ApiFieldError> Errors { get; }

    public ApiException(ProblemDetailsDto? p, List<ApiFieldError>? e, string? fallback)
        : base(fallback ?? p?.Detail ?? p?.Title)
    { Problem = p; Errors = e ?? new(); }

    static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public static async Task<ApiException> FromResponseAsync(HttpResponseMessage resp)
    {
        var fallback = resp.ReasonPhrase ?? $"HTTP {(int)resp.StatusCode}";
        var txt = await resp.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(txt))
            return new(new ProblemDetailsDto { Title = fallback, Status = (int)resp.StatusCode }, null, fallback);

        try
        {
            var env = JsonSerializer.Deserialize<ApiErrorEnvelope>(txt, _json);
            if (env is { Problem: not null } || env?.Errors?.Count > 0)
                return new(env?.Problem, env?.Errors, fallback);
        }
        catch { /* geç */ }

        try
        {
            var node = JsonNode.Parse(txt);
            var pd = new ProblemDetailsDto
            {
                Title = node?["title"]?.GetValue<string>(),
                Detail = node?["detail"]?.GetValue<string>(),
                Status = node?["status"]?.GetValue<int?>() ?? (int)resp.StatusCode
            };
            var errs = new List<ApiFieldError>();

            if (node?["errors"] is JsonArray arr)
            {
                foreach (var e in arr)
                    errs.Add(new ApiFieldError
                    {
                        Field = e?["field"]?.GetValue<string>(),
                        Code = e?["code"]?.GetValue<string>(),
                        Message = e?["message"]?.GetValue<string>()
                    });
            }
            else if (node?["errors"] is JsonObject dict)
            {
                foreach (var kv in dict)
                    if (kv.Value is JsonArray msgs)
                        foreach (var m in msgs)
                            errs.Add(new ApiFieldError { Field = kv.Key, Message = m?.GetValue<string>() });
            }

            return new(pd, errs, fallback);
        }
        catch { return new(new ProblemDetailsDto { Title = fallback, Status = (int)resp.StatusCode }, null, fallback); }
    }
}

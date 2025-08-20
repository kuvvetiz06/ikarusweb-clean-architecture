using System.Text.Json;
using System.Text.Json.Nodes;

namespace IKARUSWEB.UI.Services.Api; // kendi namespace'ini kullan

public sealed class ProblemDetailsDto
{
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
}

public sealed class ApiErrorEnvelope
{
    public ProblemDetailsDto? Problem { get; set; }
    public List<ApiFieldError>? Errors { get; set; }
}

public sealed class ApiFieldError
{
    public string? Field { get; set; }   // "name", "country" ...
    public string? Code { get; set; }    // "Validation.Tenant.Name.Required" ...
    public string? Message { get; set; } // Lokalize mesaj (TR/EN)
}

public sealed class ApiException : Exception
{
    public ProblemDetailsDto? Problem { get; }
    public List<ApiFieldError> Errors { get; }

    public ApiException(ProblemDetailsDto? p, List<ApiFieldError>? e, string? fallback)
        : base(fallback ?? p?.Detail ?? p?.Title)
    {
        Problem = p;
        Errors = e ?? new();
    }

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<ApiException> FromResponseAsync(HttpResponseMessage resp)
    {
        var fallback = resp.ReasonPhrase ?? $"HTTP {(int)resp.StatusCode}";
        var txt = await resp.Content.ReadAsStringAsync();

        // 1) Boş gövde
        if (string.IsNullOrWhiteSpace(txt))
            return new ApiException(new ProblemDetailsDto { Title = fallback, Status = (int)resp.StatusCode }, null, fallback);

        // 2) Önce bizim zarfı dene: { problem, errors[] }
        try
        {
            var env = JsonSerializer.Deserialize<ApiErrorEnvelope>(txt, _json);
            if (env is { Problem: not null } || env?.Errors?.Count > 0)
                return new ApiException(env?.Problem, env?.Errors, fallback);
        }
        catch { /* devam */ }

        // 3) RFC7807 + "errors" varyantlarını esnek parse et
        ProblemDetailsDto? problem = null;
        var errors = new List<ApiFieldError>();

        try
        {
            var node = JsonNode.Parse(txt);
            if (node is not null)
            {
                // ProblemDetails
                var title = node["title"]?.GetValue<string>();
                var detail = node["detail"]?.GetValue<string>();
                var status = node["status"]?.GetValue<int?>();

                problem = new ProblemDetailsDto
                {
                    Title = title,
                    Detail = detail,
                    Status = status ?? (int)resp.StatusCode
                };

                // errors: [{ field, code, message }]
                if (node["errors"] is JsonArray arr)
                {
                    foreach (var e in arr)
                    {
                        errors.Add(new ApiFieldError
                        {
                            Field = e?["field"]?.GetValue<string>(),
                            Code = e?["code"]?.GetValue<string>(),
                            Message = e?["message"]?.GetValue<string>()
                        });
                    }
                }
                // errors: { "FieldName": ["msg1","msg2"] }
                else if (node["errors"] is JsonObject dict)
                {
                    foreach (var kv in dict)
                    {
                        if (kv.Value is JsonArray msgs)
                        {
                            foreach (var m in msgs)
                                errors.Add(new ApiFieldError { Field = kv.Key, Message = m?.GetValue<string>() });
                        }
                    }
                }
            }
        }
        catch { /* parse hatası -> görmezden gel */ }

        return new ApiException(problem ?? new ProblemDetailsDto { Title = fallback, Status = (int)resp.StatusCode },
                                errors, fallback);
    }
}

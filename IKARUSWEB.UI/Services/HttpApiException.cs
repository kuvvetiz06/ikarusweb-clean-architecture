using System.Text.Json;
using System.Text.Json.Nodes;

namespace IKARUSWEB.UI.Services;

public sealed class HttpApiException : Exception
{
    public int StatusCode { get; }
    public List<ApiError> Errors { get; } = new();

    public HttpApiException(string message, int statusCode) : base(message) => StatusCode = statusCode;

    public static async Task<HttpApiException> FromResponseAsync(HttpResponseMessage resp)
    {
        var text = await resp.Content.ReadAsStringAsync();
        var msg = !string.IsNullOrWhiteSpace(resp.ReasonPhrase) ? resp.ReasonPhrase! : "Request failed";
        var ex = new HttpApiException(msg, (int)resp.StatusCode);

        try
        {
            if (string.IsNullOrWhiteSpace(text)) return ex;

            var node = JsonNode.Parse(text);
            if (node is null) return ex;

            // RFC7807 alanları
            var title = node["title"]?.GetValue<string>();
            var detail = node["detail"]?.GetValue<string>();
            var final = detail ?? title;
            if (!string.IsNullOrWhiteSpace(final))
            {
                var f = typeof(Exception).GetField("_message",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                f?.SetValue(ex, final);
            }

            // 1) errors: [{ field, message }]
            if (node["errors"] is JsonArray arr)
            {
                foreach (var e in arr)
                {
                    ex.Errors.Add(new ApiError
                    {
                        Field = e?["field"]?.GetValue<string>(),
                        Message = e?["message"]?.GetValue<string>()
                    });
                }
                return ex;
            }

            // 2) errors: { "Name": ["msg1","msg2"], ... }
            if (node["errors"] is JsonObject dict)
            {
                foreach (var kv in dict)
                {
                    if (kv.Value is JsonArray msgs)
                        foreach (var m in msgs)
                            ex.Errors.Add(new ApiError { Field = kv.Key, Message = m?.GetValue<string>() });
                }
                return ex;
            }
        }
        catch { /* parse hatası -> görmezden gel */ }

        return ex;
    }
}

public sealed class ApiError
{
    public string? Field { get; set; }
    public string? Message { get; set; }
}

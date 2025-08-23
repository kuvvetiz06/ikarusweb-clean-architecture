using Yarp.ReverseProxy.Forwarder;

namespace IKARUSWEB.UI.Transformers
{
    public sealed class LoginCaptureTransformer : HttpTransformer
    {
        public override async ValueTask<bool> TransformResponseAsync(
            HttpContext httpContext,
            HttpResponseMessage? proxyResponse,
            CancellationToken ct)
        {
            // proxyResponse null ise default davranışa bırak
            if (proxyResponse is null)
            {
                return await base.TransformResponseAsync(httpContext, proxyResponse, ct);
            }

            // Başarılı login?
            var status = (int)proxyResponse.StatusCode;
            if (status >= 200 && status < 300)
            {
                var body = proxyResponse.Content is null ? null : await proxyResponse.Content.ReadAsStringAsync(ct);
                var token = ExtractToken(body);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    // Token'ı session'a koy
                    httpContext.Session.SetString("access_token", token);

                    // İstemciye body göstermeden 204 dön
                    httpContext.Response.StatusCode = StatusCodes.Status204NoContent;

                    // base'i çağırma, kopyalama yapılmasın
                    return false; // <<— kritik: pipeline'ı burada bitiriyoruz
                }
            }

            // Başarısızsa veya token yoksa: default kopyalama
            return await base.TransformResponseAsync(httpContext, proxyResponse, ct);
        }

        private static string? ExtractToken(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == System.Text.Json.JsonValueKind.String)
                    return root.GetString();

                if (root.TryGetProperty("data", out var data))
                {
                    if (data.TryGetProperty("accessToken", out var at)) return at.GetString();
                    if (data.TryGetProperty("token", out var tk)) return tk.GetString();
                }
                if (root.TryGetProperty("accessToken", out var at2)) return at2.GetString();
                if (root.TryGetProperty("token", out var tk2)) return tk2.GetString();

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

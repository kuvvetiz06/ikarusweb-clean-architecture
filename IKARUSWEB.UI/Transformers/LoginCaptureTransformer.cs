using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using Yarp.ReverseProxy.Forwarder;

namespace IKARUSWEB.UI.Transformers
{
    public sealed class LoginCaptureTransformer : HttpTransformer
    {
        public override async ValueTask<bool> TransformResponseAsync(
     HttpContext http, HttpResponseMessage? proxyResponse, CancellationToken ct)
        {
            if (proxyResponse is { Content: not null } &&
                (int)proxyResponse.StatusCode is >= 200 and < 300)
            {
                var json = await proxyResponse.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                var token = data.GetProperty("accessToken").GetString();
                var expiresAt = data.GetProperty("expiresAt").GetDateTimeOffset();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    // 1) Session'a JWT
                    http.Session.SetString("access_token", token);

                    // 2) UI cookie sign-in: token exp'i claim olarak da koy
                    var claims = ExtractClaimsFromJwt(token).ToList();
                    claims.Add(new Claim("token_exp", expiresAt.ToUnixTimeSeconds().ToString()));

                    var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(id);

                    // DİKKAT: IsPersistent = true + ExpiresUtc = token süresi
                    //var props = new AuthenticationProperties
                    //{
                    //    IsPersistent = true,
                    //    ExpiresUtc = expiresAt
                    //};
                    var props = new AuthenticationProperties
                    {
                        IsPersistent = false
                        // ExpiresUtc set ETME: Cookie, CookieOptions.ExpireTimeSpan (1 saat) + sliding ile yaşasın
                    };
                    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                    http.Response.StatusCode = StatusCodes.Status200OK;
                    http.Response.ContentType = "application/json";
                    await http.Response.WriteAsync("{\"ok\":true}", ct);
                    return false; // upstream yanıtını kopyalama
                }
            }

            return await base.TransformResponseAsync(http, proxyResponse, ct);
        }


        static IEnumerable<Claim> ExtractClaimsFromJwt(string jwt)
        {
            static string Pad(string s) => s.Replace('-', '+').Replace('_', '/')
                                            .PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var parts = jwt.Split('.');
            if (parts.Length < 2) yield break;
            var payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
            using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
            var p = doc.RootElement;

            string? sub = p.TryGetProperty("nameid", out var v1) ? v1.GetString()
                        : p.TryGetProperty("sub", out var v2) ? v2.GetString()
                        : null;
            string? name = p.TryGetProperty("unique_name", out var v3) ? v3.GetString()
                         : p.TryGetProperty("name", out var v4) ? v4.GetString()
                         : null;
            string? tenantId = p.TryGetProperty("tenant_id", out var v5) ? v5.GetString() : null;

            if (!string.IsNullOrWhiteSpace(sub)) yield return new Claim(ClaimTypes.NameIdentifier, sub!);
            if (!string.IsNullOrWhiteSpace(name)) yield return new Claim(ClaimTypes.Name, name!);
            if (!string.IsNullOrWhiteSpace(tenantId)) yield return new Claim("tenant_id", tenantId!);

            if (p.TryGetProperty("role", out var rolesEl))
            {
                if (rolesEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                    foreach (var r in rolesEl.EnumerateArray())
                        if (r.ValueKind == System.Text.Json.JsonValueKind.String)
                            yield return new Claim(ClaimTypes.Role, r.GetString()!);
                        else if (rolesEl.ValueKind == System.Text.Json.JsonValueKind.String)
                            yield return new Claim(ClaimTypes.Role, rolesEl.GetString()!);
            }
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
        static DateTimeOffset? ExtractExpiresAt(string json)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("expiresAt", out var ex) &&
                    ex.TryGetDateTimeOffset(out var dto))
                    return dto;
            }
            catch { }
            return null;
        }
    }
}

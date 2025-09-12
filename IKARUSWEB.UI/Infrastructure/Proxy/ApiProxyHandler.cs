using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HttpMediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;


namespace IKARUSWEB.UI.Infrastructure.Proxy;

public static class ApiProxyHandler
{
    private static readonly JsonSerializerOptions J = new() { PropertyNameCaseInsensitive = true };

    // 1) Ana giriş: orijinal isteği upstream'e gönder; 401 gelirse refresh dene ve 1 kez retry et
    public static async Task ProxyAsync(HttpContext ctx, string apiBase, HttpMessageInvoker invoker, CancellationToken ct)
    {
        // Login/Logout/Refresh endpointleri bu handler'a düşmemeli (Program.cs'de ayrı map'liyoruz)
        var url = apiBase.TrimEnd('/') + ctx.Request.Path + ctx.Request.QueryString.Value;

        // Body'yi iki kez gönderebilelim diye bufferla (POST/PUT/PATCH)
        byte[]? body = null;
        if (HttpMethods.IsPost(ctx.Request.Method) || HttpMethods.IsPut(ctx.Request.Method) || HttpMethods.IsPatch(ctx.Request.Method))
        {
            ctx.Request.EnableBuffering();
            using var ms = new MemoryStream();
            await ctx.Request.Body.CopyToAsync(ms, ct);
            body = ms.ToArray();
            ctx.Request.Body.Position = 0;
        }

        // 1. deneme
        var res1 = await SendUpstreamAsync(ctx, url, invoker, body, attachBearer: true, ct);
        if (res1.StatusCode != System.Net.HttpStatusCode.Unauthorized)
        {
            await CopyResponseAsync(ctx, res1, ct);
            return;
        }

        // 2) 401 ise: refresh dene
        var refreshed = await TryRefreshAsync(ctx, apiBase, invoker, ct);
        if (!refreshed)
        {
            // Refresh başarısız -> UI oturumunu bitir, 401 dön
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ctx.Session.Clear();
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // 3) Başarılı refresh sonrası tek sefer retry
        var res2 = await SendUpstreamAsync(ctx, url, invoker, body, attachBearer: true, ct);
        await CopyResponseAsync(ctx, res2, ct);
    }

    // Upstream'e tek istek gönder (opsiyonel Bearer ekle)
    private static async Task<HttpResponseMessage> SendUpstreamAsync(HttpContext ctx, string url, HttpMessageInvoker invoker, byte[]? body, bool attachBearer, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), url);

        // Headers
        foreach (var (k, v) in ctx.Request.Headers)
        {
            if (k.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
            if (k.Equals("Authorization", StringComparison.OrdinalIgnoreCase)) continue; // biz set edeceğiz
            req.Headers.TryAddWithoutValidation(k, v.ToArray());
        }

        // Cookies (refresh cookie’yi upstream’e geçmekte sakınca yok)
        if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            req.Headers.TryAddWithoutValidation("Cookie", cookie.ToString());

        // Bearer
        if (attachBearer)
        {
            var token = ctx.Session.GetString("access_token");
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Body
        if (body is not null)
        {
            var contentType = ctx.Request.ContentType ?? "application/json";
            req.Content = new ByteArrayContent(body);
            req.Content.Headers.ContentType = new HttpMediaTypeHeaderValue(contentType);
        }

        // Gönder
        return await invoker.SendAsync(req, ct);
    }

    // Refresh akışı: /api/auth/refresh çağır, success ise access_token+UI cookie’yi yenile
    private static async Task<bool> TryRefreshAsync(HttpContext ctx, string apiBase, HttpMessageInvoker invoker, CancellationToken ct)
    {
        var refreshUrl = apiBase.TrimEnd('/') + "/api/auth/refresh";
        using var req = new HttpRequestMessage(HttpMethod.Post, refreshUrl);

        // Mevcut request’teki cookie’leri (HttpOnly refresh_token dahil) taşı
        if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            req.Headers.TryAddWithoutValidation("Cookie", cookie.ToString());

        using var resp = await invoker.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return false;

        // JSON: { data: { accessToken, expiresAt } }
        var json = await resp.Content.ReadAsStringAsync(ct);
        var root = JsonDocument.Parse(json).RootElement;
        var data = root.GetProperty("data");
        var token = data.GetProperty("accessToken").GetString();
        var exp = data.GetProperty("expiresAt").GetDateTimeOffset();

        if (string.IsNullOrWhiteSpace(token)) return false;

        // 1) Session'da access token'ı güncelle
        ctx.Session.SetString("access_token", token);

        // 2) UI cookie’yi güncelle (claims’i JWT’den oku; token_exp claim’i ekle)
        var claims = ExtractClaimsFromJwt(token).ToList();
        claims.RemoveAll(c => c.Type == "token_exp");
        claims.Add(new Claim("token_exp", exp.ToUnixTimeSeconds().ToString()));

        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(id);
        var props = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = exp };
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        return true;
    }

    private static IEnumerable<Claim> ExtractClaimsFromJwt(string jwt)
    {
        static string Pad(string s) => s.Replace('-', '+').Replace('_', '/').PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
        var parts = jwt.Split('.');
        if (parts.Length < 2) yield break;
        var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
        using var doc = JsonDocument.Parse(payloadJson);
        var p = doc.RootElement;

        if (p.TryGetProperty("nameid", out var v1) && !string.IsNullOrWhiteSpace(v1.GetString()))
            yield return new Claim(ClaimTypes.NameIdentifier, v1.GetString()!);
        if (p.TryGetProperty("unique_name", out var v2) && !string.IsNullOrWhiteSpace(v2.GetString()))
            yield return new Claim(ClaimTypes.Name, v2.GetString()!);
        if (p.TryGetProperty("tenant_id", out var v3) && !string.IsNullOrWhiteSpace(v3.GetString()))
            yield return new Claim("tenant_id", v3.GetString()!);

        if (p.TryGetProperty("role", out var rolesEl))
        {
            if (rolesEl.ValueKind == JsonValueKind.Array)
                foreach (var r in rolesEl.EnumerateArray())
                    if (r.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(r.GetString()))
                        yield return new Claim(ClaimTypes.Role, r.GetString()!);
                    else if (rolesEl.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(rolesEl.GetString()))
                        yield return new Claim(ClaimTypes.Role, rolesEl.GetString()!);
        }
    }

    private static async Task CopyResponseAsync(HttpContext ctx, HttpResponseMessage upstream, CancellationToken ct)
    {
        // Status code
        ctx.Response.StatusCode = (int)upstream.StatusCode;

        // Headers (genel)
        foreach (var header in upstream.Headers)
        {
            // Set-Cookie çoklu olabilir -> Append
            if (string.Equals(header.Key, HeaderNames.SetCookie, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var v in header.Value) ctx.Response.Headers.Append(header.Key, v);
            }
            else
            {
                ctx.Response.Headers[header.Key] = header.Value.ToArray();
            }
        }

        // Content headers (Content-Type, Content-Length, etc.)
        if (upstream.Content != null)
        {
            foreach (var header in upstream.Content.Headers)
            {
                if (string.Equals(header.Key, HeaderNames.ContentLength, StringComparison.OrdinalIgnoreCase))
                {
                    // Kestrel Content-Length yönetiyor; varsa set edebiliriz ama güvenli tarafta kalalım
                    ctx.Response.Headers[header.Key] = header.Value.ToArray();
                }
                else
                {
                    ctx.Response.Headers[header.Key] = header.Value.ToArray();
                }
            }
        }

        // HTTP/1.1 hop-by-hop header'ları çıkar (aksi halde Kestrel şikayet edebilir)
        ctx.Response.Headers.Remove(HeaderNames.TransferEncoding);
        ctx.Response.Headers.Remove("Connection");
        ctx.Response.Headers.Remove("Keep-Alive");
        ctx.Response.Headers.Remove("Proxy-Connection");
        ctx.Response.Headers.Remove("TE");
        ctx.Response.Headers.Remove("Trailer");
        ctx.Response.Headers.Remove("Upgrade");

        // Body
        if (upstream.Content != null)
            await upstream.Content.CopyToAsync(ctx.Response.Body, ct);
    }

    private static string? GetRefreshFromSetCookie(IEnumerable<string> setCookies)
    {
        // Örnek: "refresh_token=xxxx; Path=/api/auth; HttpOnly; Secure; SameSite=Lax; Expires=..."
        var rx = new Regex(@"(?:^|;\s*)refresh_token=([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        foreach (var sc in setCookies)
        {
            var m = rx.Match(sc);
            if (m.Success)
                return Uri.UnescapeDataString(m.Groups[1].Value);
        }
        return null;
    }

}

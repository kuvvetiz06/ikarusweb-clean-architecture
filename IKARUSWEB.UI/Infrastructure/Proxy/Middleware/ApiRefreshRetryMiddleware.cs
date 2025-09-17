using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IKARUSWEB.UI.Infrastructure.Proxy.Middleware;

public sealed class ApiRefreshRetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpMessageInvoker _invoker;
    private readonly string _apiBase;
    private readonly TimeSpan _activityTimeout;

    public ApiRefreshRetryMiddleware(
        RequestDelegate next,
        IHttpContextAccessor accessor,
        HttpMessageInvoker invoker,
        IConfiguration cfg)
    {
        _next = next;
        _invoker = invoker;

        // ReverseProxy config'inden cluster adresini ve ActivityTimeout'u oku
        _apiBase = (cfg["ReverseProxy:Clusters:ikarus-api:Destinations:d1:Address"] ??
                    cfg["Api:BaseUrl"] ??
                    "https://localhost:44340/").TrimEnd('/');

        if (TimeSpan.TryParse(cfg["ReverseProxy:Clusters:ikarus-api:HttpRequest:ActivityTimeout"], out var t))
            _activityTimeout = t;
        else
            _activityTimeout = TimeSpan.FromSeconds(90);
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        // Sadece /api/* (login/refresh/logout hariç)
        if (!ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
            ctx.Request.Path.StartsWithSegments("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
            ctx.Request.Path.StartsWithSegments("/api/auth/logout", StringComparison.OrdinalIgnoreCase) ||
            ctx.Request.Path.StartsWithSegments("/api/auth/refresh", StringComparison.OrdinalIgnoreCase))
        {
            await _next(ctx);
            return;
        }

        // Orijinal isteği upstream'e gönder
        var upstreamUrl = _apiBase + ctx.Request.Path + ctx.Request.QueryString.Value;
        var body = await BufferBodyIfNeededAsync(ctx);
        using var res1 = await SendOnceAsync(ctx, upstreamUrl, body, attachBearer: true, ctx.RequestAborted);

        if (res1.StatusCode != HttpStatusCode.Unauthorized)
        {
            await CopyResponseAsync(ctx, res1);
            return;
        }

        // 401 ise: refresh dene
        var refreshed = await TryRefreshAsync(ctx, ctx.RequestAborted);
        if (!refreshed)
        {
            // Refresh başarısız → UI oturumu bitir
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ctx.Session.Clear();
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Refresh başarılı → tek sefer retry
        using var res2 = await SendOnceAsync(ctx, upstreamUrl, body, attachBearer: true, ctx.RequestAborted);
        await CopyResponseAsync(ctx, res2);
    }

    private async Task<HttpResponseMessage> SendOnceAsync(
        HttpContext ctx, string url, byte[]? body, bool attachBearer, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_activityTimeout);

        using var req = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), url);

        // Headers (Host & Authorization hariç)
        foreach (var (k, v) in ctx.Request.Headers)
        {
            if (k.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
            if (k.Equals("Authorization", StringComparison.OrdinalIgnoreCase)) continue;
            req.Headers.TryAddWithoutValidation(k, v.ToArray());
        }

        // Cookie'leri taşı (HttpOnly refresh cookie dahil)
        if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            req.Headers.TryAddWithoutValidation("Cookie", cookie.ToString());

        if (attachBearer)
        {
            var token = ctx.Session.GetString("access_token");
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
        {
            req.Content = new ByteArrayContent(body);
            if (!string.IsNullOrEmpty(ctx.Request.ContentType))
                req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ctx.Request.ContentType);
        }

        return await _invoker.SendAsync(req, cts.Token);
    }

    private static async Task<byte[]?> BufferBodyIfNeededAsync(HttpContext ctx)
    {
        if (!(HttpMethods.IsPost(ctx.Request.Method) || HttpMethods.IsPut(ctx.Request.Method) || HttpMethods.IsPatch(ctx.Request.Method)))
            return null;

        ctx.Request.EnableBuffering();
        using var ms = new MemoryStream();
        await ctx.Request.Body.CopyToAsync(ms);
        ctx.Request.Body.Position = 0;
        return ms.ToArray();
    }

    private static async Task CopyResponseAsync(HttpContext ctx, HttpResponseMessage src)
    {
        ctx.Response.StatusCode = (int)src.StatusCode;

        foreach (var h in src.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        if (src.Content != null)
        {
            foreach (var h in src.Content.Headers)
                ctx.Response.Headers[h.Key] = h.Value.ToArray();

            ctx.Response.Headers.Remove("transfer-encoding");
            await src.Content.CopyToAsync(ctx.Response.Body);
        }
    }

    private async Task<bool> TryRefreshAsync(HttpContext ctx, CancellationToken ct)
    {
        var refreshUrl = _apiBase + "/api/auth/refresh";
        using var req = new HttpRequestMessage(HttpMethod.Post, refreshUrl);

        // HttpOnly refresh cookie'yi ilet
        if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            req.Headers.TryAddWithoutValidation("Cookie", cookie.ToString());

        using var resp = await _invoker.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return false;

        var json = await resp.Content.ReadAsStringAsync(ct);
        var root = JsonDocument.Parse(json).RootElement;
        var data = root.GetProperty("data");
        var token = data.GetProperty("accessToken").GetString();
        var exp = data.GetProperty("expiresAt").GetDateTimeOffset();

        if (string.IsNullOrWhiteSpace(token)) return false;

        // 1) Session'da access token'ı güncelle
        ctx.Session.SetString("access_token", token);

        // 2) UI cookie'yi güncelle (principal'ı JWT'den oluştur; token_exp claim’i ekle)
        var claims = ExtractClaimsFromJwt(token).ToList();
        claims.RemoveAll(c => c.Type == "token_exp");
        claims.Add(new Claim("token_exp", exp.ToUnixTimeSeconds().ToString()));

        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(id);

        // ÇOK ÖNEMLİ: Cookie’yi access token süresine bağlama (sliding çalışsın)
        var props = new AuthenticationProperties { IsPersistent = false };
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
}

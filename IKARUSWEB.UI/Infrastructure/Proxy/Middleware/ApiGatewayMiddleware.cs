using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IKARUSWEB.UI.Infrastructure.Proxy.Middleware;

public sealed class ApiGatewayMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpMessageInvoker _invoker;
    private readonly string _apiBase;
    private readonly TimeSpan _timeout;

    public ApiGatewayMiddleware(RequestDelegate next, HttpMessageInvoker invoker, IConfiguration cfg)
    {
        _next = next;
        _invoker = invoker;
        _apiBase = (cfg["ReverseProxy:Clusters:ikarus-api:Destinations:d1:Address"]
                  ?? "https://localhost:44340").TrimEnd('/');
        _timeout = TimeSpan.TryParse(cfg["ReverseProxy:Clusters:ikarus-api:HttpRequest:ActivityTimeout"], out var t)
            ? t : TimeSpan.FromSeconds(90);
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (!ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(ctx);
            return;
        }

        var path = ctx.Request.Path.Value ?? "";

        // LOGIN → upstream yanıtını yakala, Session+Cookie kur
        if (path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            await HandleLoginAsync(ctx);
            return;
        }

        // REFRESH → upstream yanıtını yakala, Session+Cookie yenile
        if (path.Equals("/api/auth/refresh", StringComparison.OrdinalIgnoreCase))
        {
            await HandleRefreshAsync(ctx);
            return;
        }

        if (path.Equals("/api/auth/logout", StringComparison.OrdinalIgnoreCase))
        {
            // Upstream’e ilet
            var upstreamUrl = _apiBase + ctx.Request.Path;
            using var res = await SendOnceAsync(ctx, upstreamUrl, await BufferBodyAsync(ctx), false, ctx.RequestAborted);

            // UI tarafında session + cookie temizle
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ctx.Session.Clear();

            // Upstream cevabı kopyala
            await CopyResponseAsync(ctx, res);
            return;
        }
        // Diğer tüm /api istekleri
        await HandleApiAsync(ctx);
    }

    private async Task HandleApiAsync(HttpContext ctx)
    {
        var url = _apiBase + ctx.Request.Path + ctx.Request.QueryString.Value;
        var body = await BufferBodyAsync(ctx);

        using var res1 = await SendOnceAsync(ctx, url, body, attachBearer: true, ctx.RequestAborted);
        if (res1.StatusCode != HttpStatusCode.Unauthorized)
        {
            await CopyResponseAsync(ctx, res1);
            return;
        }

        // 401 → refresh dene
        var refreshed = await CallRefreshAsync(ctx, ctx.RequestAborted);
        if (!refreshed)
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ctx.Session.Clear();
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Tek sefer retry
        using var res2 = await SendOnceAsync(ctx, url, body, attachBearer: true, ctx.RequestAborted);
        await CopyResponseAsync(ctx, res2);
    }

    private async Task HandleLoginAsync(HttpContext ctx)
    {
        var upstreamUrl = _apiBase + ctx.Request.Path;
        using var res = await SendOnceAsync(ctx, upstreamUrl, await BufferBodyAsync(ctx), false, ctx.RequestAborted);
        var json = await res.Content.ReadAsStringAsync(ctx.RequestAborted);
        if (!res.IsSuccessStatusCode) { ctx.Response.StatusCode = (int)res.StatusCode; return; }

        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        var token = data.GetProperty("accessToken").GetString();
        var exp = data.GetProperty("expiresAt").GetDateTimeOffset();

        if (!string.IsNullOrWhiteSpace(token))
        {
            ctx.Session.SetString("access_token", token);

            var claims = ExtractClaimsFromJwt(token).ToList();
            claims.Add(new Claim("token_exp", exp.ToUnixTimeSeconds().ToString()));

            var tenantName = claims.FirstOrDefault(c => c.Type == "tenant_name")?.Value;
               if (!string.IsNullOrWhiteSpace(tenantName))
                ctx.Session.SetString("tenant_name", tenantName!);
            var fullName = claims.FirstOrDefault(c => c.Type == "full_name")?.Value;
            if (!string.IsNullOrWhiteSpace(fullName))
                ctx.Session.SetString("full_name", fullName!);

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(id);

            // Persistent değil → tarayıcı kapanınca çıkar
            var props = new AuthenticationProperties { IsPersistent = false };
            await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        }

        ctx.Response.StatusCode = StatusCodes.Status200OK;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync("{\"ok\":true}", ctx.RequestAborted);
    }

    private async Task HandleRefreshAsync(HttpContext ctx)
    {
        var upstreamUrl = _apiBase + ctx.Request.Path;
        using var res = await SendOnceAsync(ctx, upstreamUrl, await BufferBodyAsync(ctx), false, ctx.RequestAborted);
        var json = await res.Content.ReadAsStringAsync(ctx.RequestAborted);
        if (!res.IsSuccessStatusCode) { await CopyResponseAsync(ctx, res); return; }

        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        var token = data.GetProperty("accessToken").GetString();
        var exp = data.GetProperty("expiresAt").GetDateTimeOffset();

        if (!string.IsNullOrWhiteSpace(token))
        {
            ctx.Session.SetString("access_token", token);

            var claims = ExtractClaimsFromJwt(token).ToList();
            claims.RemoveAll(c => c.Type == "token_exp");
            claims.Add(new Claim("token_exp", exp.ToUnixTimeSeconds().ToString()));

            var tenantName = claims.FirstOrDefault(c => c.Type == "tenant_name")?.Value;
            if (!string.IsNullOrWhiteSpace(tenantName))
                ctx.Session.SetString("tenant_name", tenantName!);
            var fullName = claims.FirstOrDefault(c => c.Type == "full_name")?.Value;
            if (!string.IsNullOrWhiteSpace(fullName))
                ctx.Session.SetString("full_name", fullName!);

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(id);
            var props = new AuthenticationProperties { IsPersistent = false };
            await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        }

        await CopyResponseAsync(ctx, res);
    }

    private async Task<bool> CallRefreshAsync(HttpContext ctx, CancellationToken ct)
    {
        var url = _apiBase + "/api/auth/refresh";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            req.Headers.TryAddWithoutValidation("Cookie", cookie.ToString());

        using var res = await _invoker.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return false;

        var json = await res.Content.ReadAsStringAsync(ct);
        var root = JsonDocument.Parse(json).RootElement;
        var data = root.GetProperty("data");
        var token = data.GetProperty("accessToken").GetString();
        var exp = data.GetProperty("expiresAt").GetDateTimeOffset();

        if (string.IsNullOrWhiteSpace(token)) return false;

        ctx.Session.SetString("access_token", token);

        var claims = ExtractClaimsFromJwt(token).ToList();
        claims.RemoveAll(c => c.Type == "token_exp");
        claims.Add(new Claim("token_exp", exp.ToUnixTimeSeconds().ToString()));

        var tenantName = claims.FirstOrDefault(c => c.Type == "tenant_name")?.Value;
        if (!string.IsNullOrWhiteSpace(tenantName))
            ctx.Session.SetString("tenant_name", tenantName!);
        var fullName = claims.FirstOrDefault(c => c.Type == "full_name")?.Value;
        if (!string.IsNullOrWhiteSpace(fullName))
            ctx.Session.SetString("full_name", fullName!);

        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(id);
        var props = new AuthenticationProperties { IsPersistent = false };
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        return true;
    }

    private async Task<HttpResponseMessage> SendOnceAsync(HttpContext ctx, string url, byte[]? body, bool attachBearer, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);

        var req = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), url);
        foreach (var (k, v) in ctx.Request.Headers)
        {
            if (k.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
            if (k.Equals("Authorization", StringComparison.OrdinalIgnoreCase)) continue;
            req.Headers.TryAddWithoutValidation(k, v.ToArray());
        }
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

    private static async Task<byte[]?> BufferBodyAsync(HttpContext ctx)
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
        foreach (var h in src.Headers) ctx.Response.Headers[h.Key] = h.Value.ToArray();
        if (src.Content != null)
        {
            foreach (var h in src.Content.Headers) ctx.Response.Headers[h.Key] = h.Value.ToArray();
            ctx.Response.Headers.Remove("transfer-encoding");
            await src.Content.CopyToAsync(ctx.Response.Body);
        }
    }

    private static IEnumerable<Claim> ExtractClaimsFromJwt(string jwt)
    {
        static string Pad(string s) => s.Replace('-', '+').Replace('_', '/').PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
        var parts = jwt.Split('.');
        if (parts.Length < 2) yield break;
        var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
        using var doc = JsonDocument.Parse(payloadJson);
        var p = doc.RootElement;

        if (p.TryGetProperty("nameid", out var v1)) yield return new Claim(ClaimTypes.NameIdentifier, v1.GetString() ?? "");
        if (p.TryGetProperty("unique_name", out var v2)) yield return new Claim(ClaimTypes.Name, v2.GetString() ?? "");
        if (p.TryGetProperty("tenant_id", out var v3)) yield return new Claim("tenant_id", v3.GetString() ?? "");
        if (p.TryGetProperty("tenant_name", out var v4)) yield return new Claim("tenant_name", v4.GetString() ?? "");
        if (p.TryGetProperty("full_name", out var v5)) yield return new Claim("full_name", v5.GetString() ?? "");

        if (p.TryGetProperty("role", out var rolesEl))
        {
            if (rolesEl.ValueKind == JsonValueKind.Array)
                foreach (var r in rolesEl.EnumerateArray())
                    if (r.ValueKind == JsonValueKind.String)
                        yield return new Claim(ClaimTypes.Role, r.GetString() ?? "");
                    else if (rolesEl.ValueKind == JsonValueKind.String)
                        yield return new Claim(ClaimTypes.Role, rolesEl.GetString() ?? "");
        }
    }
}

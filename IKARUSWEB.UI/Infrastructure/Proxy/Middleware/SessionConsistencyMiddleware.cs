using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IKARUSWEB.UI.Infrastructure.Proxy.Middleware;

public sealed class SessionConsistencyMiddleware
{
    private readonly RequestDelegate _next;

    public SessionConsistencyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        var path = ctx.Request.Path.Value ?? string.Empty;

        // /api/auth/* isteklerine karışma (login/refresh/logout)
        if (path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(ctx);
            return;
        }

        var isAuthenticated = ctx.User?.Identity?.IsAuthenticated == true;
        var hasSessionToken = !string.IsNullOrEmpty(ctx.Session.GetString("access_token"));

        if (isAuthenticated && !hasSessionToken)
        {
            // Session boş → auth cookie restore edilmiş = logout
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ctx.Session.Clear();

            if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            ctx.Response.Redirect("/account/login");
            return;
        }

        await _next(ctx);
    }
}

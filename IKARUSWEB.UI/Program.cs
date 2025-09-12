using IKARUSWEB.UI.Filters;
using IKARUSWEB.UI.Helper;
using IKARUSWEB.UI.Infrastructure;
using IKARUSWEB.UI.Infrastructure.Auth;
using IKARUSWEB.UI.Infrastructure.Proxy;
using IKARUSWEB.UI.Transformers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);

// --- Redis var mý? ---
var redisConn = builder.Configuration["Redis:Configuration"];
var useRedis = !string.IsNullOrWhiteSpace(redisConn);
var ticketPrefix = (builder.Configuration["Redis:InstanceName"] ?? "ikarusweb:") + "auth:";
// --- Cache/Session ---
if (useRedis)
{
    builder.Services.AddStackExchangeRedisCache(o =>
    {
        o.Configuration = redisConn;                       // örn: "localhost:6379,abortConnect=false"
        o.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "ikarusweb:";
    });

    // DataProtection anahtarlarýný paylaþýmlý tutmak sadece çoklu instance/prod için gerekli
    var cfg = ConfigurationOptions.Parse(redisConn);
    cfg.AbortOnConnectFail = false;
    var mux = ConnectionMultiplexer.Connect(cfg);
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(mux, "ikarusweb:keys");
}
else
{
    // DEV fallback
    builder.Services.AddDistributedMemoryCache();
}
// Localization
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supported = new[] { "tr-TR", "en-US" }
        .Select(c => new CultureInfo(c)).ToArray();
    options.DefaultRequestCulture = new("tr-TR");
    options.SupportedCultures = supported;
    options.SupportedUICultures = supported;

});

builder.Services.AddControllersWithViews(o =>
{
    o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    // Global authorize için **ya bunu kullan, ya FallbackPolicy** (ikisi birden deðil)
    o.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build()));
})
.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddDataAnnotationsLocalization()
.AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null)
.AddViewOptions(y => y.HtmlHelperOptions.ClientValidationEnabled = true)
.AddNewtonsoftJson(z => z.SerializerSettings.ContractResolver = new DefaultContractResolver())
.AddRazorRuntimeCompilation();
// 3.4 Session (Redis-backed)
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".ikarusweb.sid";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.IdleTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = ".ikarusweb.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.Cookie.Path = "/";
        o.LoginPath = "/account/login";
        o.LogoutPath = "/account/logout";
        o.AccessDeniedPath = "/account/denied";
        o.SlidingExpiration = false;
        o.ExpireTimeSpan = TimeSpan.FromHours(1);

        o.Events = new CookieAuthenticationEvents
        {
            // Her istekten belirli aralýklarla cookie'yi doðrula
            OnValidatePrincipal = ctx =>
            {
                var expStr = ctx.Principal?.FindFirst("token_exp")?.Value;
                if (long.TryParse(expStr, out var unix))
                {
                    var exp = DateTimeOffset.FromUnixTimeSeconds(unix);
                    if (DateTimeOffset.UtcNow >= exp)
                    {
                        // Token süresi geçti: UI cookie'yi düþür
                        ctx.RejectPrincipal();
                        return ctx.HttpContext.SignOutAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                }
                return Task.CompletedTask;
            },

            // /api isteklerinde 302 yerine 401
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };

        if (useRedis)
        {
            var cache = builder.Services.BuildServiceProvider().GetRequiredService<IDistributedCache>();
            o.SessionStore = new RedisTicketStore(cache, ticketPrefix);
        }
    });

// YARP forwarder için HttpMessageInvoker (YARP’ýn önerdiði pattern)
builder.Services.AddSingleton<HttpMessageInvoker>(_ =>
{
    var handler = new SocketsHttpHandler
    {
        UseProxy = false,
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.All
    };
#if DEBUG
    handler.SslOptions = new SslClientAuthenticationOptions
    {
        RemoteCertificateValidationCallback = static (_, _, _, _) => true
    };
#endif
    return new HttpMessageInvoker(handler, disposeHandler: true);
});

builder.Services.AddReverseProxy();


var app = builder.Build();

// RequestLocalization
app.UseRequestLocalization();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api") &&
        !ctx.Request.Path.StartsWithSegments("/api/auth/login") &&
        !ctx.Request.Path.StartsWithSegments("/api/auth/logout") &&
        !ctx.Request.Path.StartsWithSegments("/api/auth/refresh"))
    {
        // Session'dan token oku
        var token = ctx.Session.GetString("access_token");
        if (!string.IsNullOrWhiteSpace(token))
        {
            // JWT exp'ini hýzlýca decode et (imza doðrulamadan)
            var exp = GetJwtExpires.Get(token); // DateTimeOffset?
            if (exp.HasValue && DateTimeOffset.UtcNow >= exp.Value)
            {
                // Süresi bitti: UI'ý düþür, 401 dön
                await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ctx.Session.Clear();
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            ctx.Request.Headers.Authorization = $"Bearer {token}";
        }
    }
    await next();
});


var apiBase = (app.Configuration["Api:BaseUrl"] ?? "https://localhost:44340").TrimEnd('/'); 
var invoker = app.Services.GetRequiredService<HttpMessageInvoker>();
var forwarder = app.Services.GetRequiredService<IHttpForwarder>();
var fwdCfg = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
// Controller route
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");



// Login özel endpoint (ÖNCE)
app.MapPost("/api/auth/login", async ctx =>
{
    var err = await forwarder.SendAsync(ctx, apiBase, invoker, fwdCfg, new LoginCaptureTransformer());
    if (err != ForwarderError.None) ctx.Response.StatusCode = 502;
}).AllowAnonymous();



// Logout
app.MapPost("/api/auth/logout", async context =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    context.Session.Remove("access_token");
    context.Session.Clear();
    var err = await forwarder.SendAsync(context, apiBase, invoker, fwdCfg);
    if (err != ForwarderError.None) context.Response.StatusCode = StatusCodes.Status204NoContent;
}).AllowAnonymous();

// Genel /api proxy (SONRA)
app.Map("/api/{**catch-all}", async ctx =>
{
    await ApiProxyHandler.ProxyAsync(ctx, apiBase, invoker, ctx.RequestAborted);
}).AllowAnonymous();






app.Run();



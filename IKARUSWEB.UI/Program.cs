//UI.Program.cs 
using IKARUSWEB.UI.Helper;
using IKARUSWEB.UI.Infrastructure;
using IKARUSWEB.UI.Infrastructure.Proxy;
using IKARUSWEB.UI.Infrastructure.Proxy.Middleware;
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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDistributedMemoryCache();

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
    // Global authorize için **ya bunu kullan, ya FallbackPolicy** (ikisi birden değil)
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
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromHours(1);

        // /api isteklerinde 302→401 davranışı kalsın istiyorsan yalnızca bu kısmı bırak:
        o.Events = new CookieAuthenticationEvents
        {
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
    });

// YARP forwarder için HttpMessageInvoker (YARP’ın önerdiği pattern)
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

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<AccessTokenTransform>();


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
app.UseMiddleware<ApiRefreshRetryMiddleware>();

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

// Refresh (artık özel map'li - RefreshCaptureTransformer devrede)
app.MapPost("/api/auth/refresh", async ctx =>
{
    var err = await forwarder.SendAsync(ctx, apiBase, invoker, fwdCfg, new RefreshCaptureTransformer());
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

app.MapReverseProxy();

app.Run();



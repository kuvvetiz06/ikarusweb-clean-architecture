using IKARUSWEB.UI.Filters;
using IKARUSWEB.UI.Services;
using IKARUSWEB.UI.Services.Api;
using IKARUSWEB.UI.Transformers;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);



// Localization
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
var supported = new[] { "tr-TR", "en-US" }.Select(c => new CultureInfo(c)).ToArray();
var rlo = new RequestLocalizationOptions
{
    DefaultRequestCulture = new("tr-TR"),
    SupportedCultures = supported,
    SupportedUICultures = supported,
    RequestCultureProviders = new IRequestCultureProvider[] {
    new QueryStringRequestCultureProvider(),
    new CookieRequestCultureProvider(),
    new AcceptLanguageHeaderRequestCultureProvider()
  }
};
builder.Services.AddControllersWithViews(o =>
{
    o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    o.Filters.Add<UnauthorizedRedirectFilter>();
}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix).AddDataAnnotationsLocalization();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".ikarusweb.sid";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.IdleTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddHttpContextAccessor();
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
builder.Services.AddSession();
builder.Services.AddSingleton<ITempDataNotifier, TempDataNotifier>();
builder.Services.AddTransient<AuthTokenHandler>();


var app = builder.Build();

// RequestLocalization
app.UseRequestLocalization(rlo);

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

var apiBase = (app.Configuration["Api:BaseUrl"] ?? "https://localhost:44340").TrimEnd('/'); 
var invoker = app.Services.GetRequiredService<HttpMessageInvoker>();
var forwarder = app.Services.GetRequiredService<IHttpForwarder>();
var fwdCfg = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 2) Login özel forwarder (token'ý Session'a yazan transformer)
app.MapPost("/api/auth/login", async context =>
{
    var transformer = new LoginCaptureTransformer();
    var err = await forwarder.SendAsync(context, apiBase, invoker, fwdCfg, transformer);
    if (err != ForwarderError.None) context.Response.StatusCode = StatusCodes.Status502BadGateway;
});

// 3) Logout (Session temizle + upstream’e ilet)
app.MapPost("/api/auth/logout", async context =>
{
    context.Session.Remove("access_token");
    var err = await forwarder.SendAsync(context, apiBase, invoker, fwdCfg);
    if (err != ForwarderError.None) context.Response.StatusCode = StatusCodes.Status204NoContent;
});

// 4) SADECE /api/** yollarýný proxy'le (catch-all DEÐÝL!)
app.Map("/api/{**catch-all}", async ctx =>
{
#if DEBUG
    ctx.Response.Headers["X-Proxy-Info"] = $"{apiBase}{ctx.Request.Path}{ctx.Request.QueryString}";
#endif
    var err = await forwarder.SendAsync(ctx, apiBase, invoker, fwdCfg);
    if (err != ForwarderError.None) ctx.Response.StatusCode = StatusCodes.Status502BadGateway;
});



app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api") &&
        !ctx.Request.Path.StartsWithSegments("/api/auth/login") &&
        !ctx.Request.Path.StartsWithSegments("/api/auth/logout"))
    {
        var token = ctx.Session.GetString("access_token");
        if (!string.IsNullOrWhiteSpace(token))
            ctx.Request.Headers.Authorization = $"Bearer {token}";
    }
    await next();
});




app.Run();



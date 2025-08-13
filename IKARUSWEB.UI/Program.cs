using IKARUSWEB.UI.Filters;
using IKARUSWEB.UI.Models.Api;
using IKARUSWEB.UI.Services.Api;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);



// Localization
// 1) .resx kök klasörü
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var locCfg = builder.Configuration.GetSection("Localization");
var supported = locCfg.GetSection("SupportedCultures").Get<string[]>() ?? new[] { "tr-TR", "en-US" };
var defaultCulture = locCfg["DefaultCulture"] ?? "tr-TR";
var cultures = supported.Select(c => new CultureInfo(c)).ToArray();
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add<UnauthorizedRedirectFilter>();
}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
// Cookie auth
builder.Services.AddAuthentication("ui-cookie")
    .AddCookie("ui-cookie", o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/Denied";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient<IApiClient, ApiClient>(http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Api:BaseAddress"]!);
})
.AddHttpMessageHandler<AuthTokenHandler>()
.AddPolicyHandler(HttpPolicies.GetRetryPolicy())
.AddPolicyHandler(HttpPolicies.GetTimeoutPolicy());

var app = builder.Build();

// RequestLocalization
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new(defaultCulture),
    SupportedCultures = cultures,
    SupportedUICultures = cultures
});

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

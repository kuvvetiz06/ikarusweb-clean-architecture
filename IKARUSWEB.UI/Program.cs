using IKARUSWEB.UI.Filters;
using IKARUSWEB.UI.Services;
using IKARUSWEB.UI.Services.Api;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;

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

builder.Services.AddSingleton<ITempDataNotifier, TempDataNotifier>();
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
app.UseRequestLocalization(rlo);

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

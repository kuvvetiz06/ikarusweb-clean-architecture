//UI.Program.cs 
using IKARUSWEB.UI.Helper;
using IKARUSWEB.UI.Infrastructure;
using IKARUSWEB.UI.Infrastructure.Proxy;
using IKARUSWEB.UI.Infrastructure.Proxy.Middleware;
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

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".ikarusweb.session";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.IdleTimeout = TimeSpan.FromMinutes(60);
});



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = ".ikarusweb.auth";
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromHours(1);
        o.LoginPath = "/account/login";
        o.LogoutPath = "/account/logout";
        o.AccessDeniedPath = "/account/denied";
        o.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddSingleton(sp =>
{
    var handler = new SocketsHttpHandler
    {
        UseCookies = false,
        AllowAutoRedirect = false
    };
    return new HttpMessageInvoker(handler);
});
builder.Services.AddHttpContextAccessor();


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
app.UseMiddleware<SessionConsistencyMiddleware>();
app.UseMiddleware<ApiGatewayMiddleware>();


// Controller route
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");


app.Run();



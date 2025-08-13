using FluentValidation;
using IKARUSWEB.Application.Behaviors;
using IKARUSWEB.Application.Features.Tenants.Commands.CreateTenant;
using IKARUSWEB.Application.Mapping;
using IKARUSWEB.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
// Localization (system messages)
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


// JWT Auth
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? "");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
// Application registrations
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateTenantCommand).Assembly));


builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(TenantProfile).Assembly); // Application içindeki profilleri yükler
});

// FluentValidation: Application assembly içindeki validator'larý yükle
builder.Services.AddValidatorsFromAssembly(typeof(CreateTenantCommand).Assembly);

// Pipeline behaviors
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Infrastructure (EF Core, Interceptor, Repositories, ICurrentUser)
builder.Services.AddInfrastructure(builder.Configuration);

// CORS: UI API'yi tüketecek
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("ui", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true));
});
builder.Services.AddTransient<IKARUSWEB.API.Middlewares.ExceptionMiddleware>();
var app = builder.Build();

// RequestLocalization
app.UseRequestLocalization(rlo);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();            // /openapi/v1.json
    app.MapScalarApiReference(); // /scalar/v1
}

app.UseMiddleware<IKARUSWEB.API.Middlewares.ExceptionMiddleware>();
app.UseHttpsRedirection();

// JWT/Identity sonraki PR’da
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("ui");
app.MapControllers();

app.Run();

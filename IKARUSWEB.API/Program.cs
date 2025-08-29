using FluentValidation;
using IKARUSWEB.API.Transformers;
using IKARUSWEB.Application.Behaviors;
using IKARUSWEB.Application.Features.Tenants.Commands.CreateTenant;
using IKARUSWEB.Application.Mapping;
using IKARUSWEB.Infrastructure;
using IKARUSWEB.Infrastructure.Seed;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var supported = new[] { "tr-TR", "en-US" }.Select(c => new CultureInfo(c)).ToArray();
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? "");


// Localization (system messages)
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

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

builder.Services.AddControllers();

// OpenAPI (Microsoft.AspNetCore.OpenApi) + JWT security eklemek için transformer
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(new JwtBearerSecurityTransformer());
});

builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    // ModelState invalid ise otomatik 400 dönmesin; handler çalýþsýn,
    // FluentValidation ValidationException’ýný ExceptionMiddleware yakalasýn.
    o.SuppressModelStateInvalidFilter = true;
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


builder.Services.AddTransient<IKARUSWEB.API.Middlewares.ExceptionMiddleware>();
var app = builder.Build();

// RequestLocalization
app.UseRequestLocalization(rlo);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "IKARUSWEB API";
        // Ýstersen tema/varsayýlanlarý burada deðiþtirebilirsin
    });


    //Dummy data
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevSeeder>();
    await seeder.RunAsync();
}

app.UseMiddleware<IKARUSWEB.API.Middlewares.ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

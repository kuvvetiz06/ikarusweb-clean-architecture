using FluentValidation;
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

builder.Services.AddControllers();

//Open API
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        // SecuritySchemes
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Bearer {token}"
        };

        // Global security requirement (NOT: Security -> SecurityRequirements)
        document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }
            ] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });
});

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

builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    // ModelState invalid ise otomatik 400 dönmesin; handler çalýþsýn,
    // FluentValidation ValidationException’ýný ExceptionMiddleware yakalasýn.
    o.SuppressModelStateInvalidFilter = true;
});

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

    //Dummy data
    //using var scope = app.Services.CreateScope();
    //var seeder = scope.ServiceProvider.GetRequiredService<DevSeeder>();
    //await seeder.RunAsync();
}

app.UseMiddleware<IKARUSWEB.API.Middlewares.ExceptionMiddleware>();
app.UseHttpsRedirection();

// JWT/Identity sonraki PR’da
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("ui");
app.MapControllers();

app.Run();


using FluentValidation;
using IKARUSWEB.API.Middlewares;
using IKARUSWEB.API.Transformers;
using IKARUSWEB.Application.Behaviors;
using IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType;
using IKARUSWEB.Application.Features.Tenants.Commands.CreateTenant;
using IKARUSWEB.Application.Features.Tenants.Mapping;
using IKARUSWEB.Infrastructure;
using IKARUSWEB.Infrastructure.Seed;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Context;
using Serilog.Debugging;
using Serilog.Events;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) Serilog'u appsettings.json'dan oku
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// 2) Host'a Serilog'u baðla (Build'ten önce!)
builder.Host.UseSerilog();

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? "");

// Localization (system messages)
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");



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
builder.Services.AddValidatorsFromAssemblyContaining<CreateRoomBedTypeCommandValidator>();

// Pipeline behaviors
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TenantScopeBehavior<,>));
builder.Services.AddScoped<ResultLocalizationFilter>();
// Infrastructure (EF Core, Interceptor, Repositories, ICurrentUser)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultLocalizationFilter>(); // GLOBAL ekle
});

var app = builder.Build();
app.Use(async (ctx, next) =>
{
    var id = ctx.Request.Headers.TryGetValue("X-Correlation-ID", out var cid) && !string.IsNullOrWhiteSpace(cid)
        ? cid.ToString()
        : Activity.Current?.Id ?? Guid.NewGuid().ToString("N");

    ctx.Response.Headers["X-Correlation-ID"] = id;
    using (LogContext.PushProperty("CorrelationId", id))
        await next();
});
app.UseSerilogRequestLogging(opts =>
{
    opts.GetLevel = (httpCtx, elapsedSeconds, ex) =>
    {
        // elapsedSeconds: double (saniye)
        var ms = elapsedSeconds * 1000;

        var p = httpCtx.Request.Path.Value ?? string.Empty;
        if (p.StartsWith("/health") || p.StartsWith("/scalar") || p.Contains(".css") || p.Contains(".js"))
            return Serilog.Events.LogEventLevel.Verbose;

        if (ex != null || httpCtx.Response.StatusCode >= 500) return Serilog.Events.LogEventLevel.Error;
        if (httpCtx.Response.StatusCode >= 400) return Serilog.Events.LogEventLevel.Warning;
        return ms > 2000 ? Serilog.Events.LogEventLevel.Information : Serilog.Events.LogEventLevel.Debug;
    };
});
// RequestLocalization
app.UseRequestLocalization(o =>
{
    var cultures = new[] { "tr", "en" };
    o.SetDefaultCulture("tr")
     .AddSupportedCultures(cultures)
     .AddSupportedUICultures(cultures);
});

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

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Services.GetRequiredService<ILogger<Program>>()
   .LogInformation("Serilog app-.log smoke test (Program)");
app.Run();

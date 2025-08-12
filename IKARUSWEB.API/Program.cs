using FluentValidation;
using IKARUSWEB.Application.Behaviors;
using IKARUSWEB.Application.Features.Tenants.Commands.CreateTenant;
using IKARUSWEB.Application.Mapping;
using IKARUSWEB.Infrastructure;
using MediatR;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Localization (system messages)
builder.Services.AddLocalization();
var locCfg = builder.Configuration.GetSection("Localization");
var supported = locCfg.GetSection("SupportedCultures").Get<string[]>() ?? new[] { "tr-TR", "en-US" };
var defaultCulture = locCfg["DefaultCulture"] ?? "tr-TR";
var cultures = supported.Select(c => new CultureInfo(c)).ToArray();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "IKARUSWEB API", Version = "v1" });
    // JWT �emas�n� bir sonraki PR�da ekleyece�iz
});

// Application registrations
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateTenantCommand).Assembly));


builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(TenantProfile).Assembly); // Application i�indeki profilleri y�kler
});

// FluentValidation: Application assembly i�indeki validator'lar� y�kle
builder.Services.AddValidatorsFromAssembly(typeof(CreateTenantCommand).Assembly);

// Pipeline behaviors
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Infrastructure (EF Core, Interceptor, Repositories, ICurrentUser)
builder.Services.AddInfrastructure(builder.Configuration);

// CORS: UI API'yi t�ketecek
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("ui", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true));
});
builder.Services.AddTransient<IKARUSWEB.API.Middlewares.ExceptionMiddleware>();
var app = builder.Build();

// RequestLocalization
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new(defaultCulture),
    SupportedCultures = cultures,
    SupportedUICultures = cultures
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<IKARUSWEB.API.Middlewares.ExceptionMiddleware>();
app.UseHttpsRedirection();

// JWT/Identity sonraki PR�da
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("ui");
app.MapControllers();

app.Run();

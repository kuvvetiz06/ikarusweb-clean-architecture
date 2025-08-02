using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using IKARUSWEB.Application.Common.Interfaces;
using IKARUSWEB.Infrastructure.Identity;
using IKARUSWEB.Infrastructure.Persistence;
using IKARUSWEB.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace IKARUSWEB.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Multi-tenant
            services.AddMultiTenant<TenantInfo>()
                    .WithHeaderStrategy("X-Tenant-ID");

            // DbContext (tenant bazlı)
            services.AddDbContext<IKARUSWEBDbContext>((sp, opts) =>
            {
                var accessor = sp.GetRequiredService<IMultiTenantContextAccessor>();
                var info = accessor.MultiTenantContext?.TenantInfo
                           as Infrastructure.Multitenancy.TenantInfo
                           ?? throw new InvalidOperationException("Tenant info bulunamadı.");
                opts.UseSqlServer(info.ConnectionString);
            });

            // Repository
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            // Identity & JWT
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                    .AddDefaultTokenProviders();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = true;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidIssuer = configuration["Jwt:Issuer"],
                            ValidAudience = configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                        };
                    });

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IKARUSWEB API", Version = "v1" });
            });

            // JSON options
            services.AddControllers().AddNewtonsoftJson();

            // Localization
            services.AddLocalization();

            return services;
        }
    }
}

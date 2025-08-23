using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Abstractions.Security;
using IKARUSWEB.Infrastructure.Auth;
using IKARUSWEB.Infrastructure.Identity;
using IKARUSWEB.Infrastructure.Persistence;
using IKARUSWEB.Infrastructure.Persistence.Interceptors;
using IKARUSWEB.Infrastructure.Persistence.Repositories;
using IKARUSWEB.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using tokenOptions = IKARUSWEB.Application.Common.Security;

namespace IKARUSWEB.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var cs = config.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection not found.");

            services.AddIdentityCore<AppUser>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<ITenantProvider, CurrentTenant>();
            services.AddSingleton<IDateTime, SystemDateTime>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<AuditingSaveChangesInterceptor>();
            services.AddScoped<TenantAssignmentInterceptor>();

            //services.AddScoped<DevSeeder>();

            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>()); // SaveChanges için

            services.Configure<tokenOptions.TokenOptions>(config.GetSection("Jwt"));
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddDbContext<AppDbContext>((sp, opt) =>
            {
                opt.UseSqlServer(cs, sql =>
                {
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });

                opt.AddInterceptors(sp.GetRequiredService<AuditingSaveChangesInterceptor>());
                opt.AddInterceptors(sp.GetRequiredService<TenantAssignmentInterceptor>());
            });

            // IAppDbContext portu (repo’lar commit için kullanacak)
            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            return services;
        }
    }

    internal sealed class SystemDateTime : IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }



}

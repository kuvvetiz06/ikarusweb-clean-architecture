using IKARUSWEB.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Persistence
{
    public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../IKARUSWEB.API");
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true) // varsa Development ayarlarını da ekle
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            // Basit clock
            var tenant = new DesignTimeTenantProvider();

            return new AppDbContext(optionsBuilder.Options, tenant);
        }

        private sealed class DesignTimeClock : IDateTime
        {
            public DateTime UtcNow => DateTime.UtcNow;
        }

        private sealed class DesignTimeTenantProvider : ITenantProvider
        {
            public Guid TenantId => Guid.Empty;   // design-time'da tenant yok
            public bool IsSuperUser => true; // filtreleri etkisiz bırakmaya uygun
            public bool IsResolved => false;
        }
    }

}

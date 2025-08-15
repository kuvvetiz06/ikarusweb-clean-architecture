using IKARUSWEB.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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

            // Örnek fallback. Gerçekte API/appsettings.json kullanılacak.
            var cs = Environment.GetEnvironmentVariable("IKARUSWEB_ConnectionString")
                     ?? "Server = HUSEYINGOKDEMR\\SQLEXPRESS; TrustServerCertificate=True; Database = IKARUSWEB; User Id = sa; Password = Gok1905demir+";

            optionsBuilder.UseSqlServer(cs);

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
            public Guid? TenantId => null;   // design-time'da tenant yok
            public bool IsSuperUser => true; // filtreleri etkisiz bırakmaya uygun
        }
    }

}

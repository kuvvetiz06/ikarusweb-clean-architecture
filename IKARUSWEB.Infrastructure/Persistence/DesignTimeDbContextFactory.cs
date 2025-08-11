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
                     ?? "Server = HUSEYINGOKDEMR\\SQLEXPRESS; TrustServerCertificate=True; Database = MailScheduler; User Id = sa; Password = Gok1905demir+";

            optionsBuilder.UseSqlServer(cs);

            // Basit clock
            IDateTime clock = new DesignTimeClock();

            return new AppDbContext(optionsBuilder.Options, clock);
        }

        private sealed class DesignTimeClock : IDateTime
        {
            public DateTime UtcNow => DateTime.UtcNow;
        }
    }
}

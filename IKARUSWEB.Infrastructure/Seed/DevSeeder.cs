using IKARUSWEB.Domain.Entities;
using IKARUSWEB.Infrastructure.Identity;
using IKARUSWEB.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Seed
{
    public sealed class DevSeeder
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _users;
        private readonly RoleManager<IdentityRole<Guid>> _roles;

        public DevSeeder(AppDbContext db, UserManager<AppUser> users, RoleManager<IdentityRole<Guid>> roles)
        { _db = db; _users = users; _roles = roles; }

        public async Task RunAsync()
        {
            await _db.Database.MigrateAsync();

            // 1) Tenant yoksa, default currency’siz oluştur
            var tenant = await _db.Set<Tenant>().FirstOrDefaultAsync();
            if (tenant is null)
            {
                tenant = new Tenant(
                    name: "IKARUS HOTEL",
                    country: "Türkiye",
                    city: "Ankara",
                    street: "Kızılay",
                    timeZone: "Europe/Istanbul",
                    defaultCulture: "tr-TR"
                );
                _db.Add(tenant);
                await _db.SaveChangesAsync(); // <-- Önce tenant id’yi alalım
            }

            // 2) Tenant’a bağlı currency’ler yoksa ekle
            if (!await _db.Set<Currency>().AnyAsync(c => c.TenantId == tenant.Id))
            {
                _db.AddRange(
                    new Currency(tenant.Id, "Türk Lirası", "TRY", 1m, 1m),
                    new Currency(tenant.Id, "Amerikan Doları", "USD", 1m, 35.250000m),
                    new Currency(tenant.Id, "Euro", "EUR", 1m, 38.600000m)
                );
                await _db.SaveChangesAsync();
            }

            // 3) Tenant.DefaultCurrencyCode boşsa TRY olarak set et
            if (string.IsNullOrWhiteSpace(tenant.DefaultCurrencyCode))
            {
                tenant.SetDefaultCurrency("TRY");
                _db.Update(tenant);
                await _db.SaveChangesAsync();
            }

            // 4) Rol ve kullanıcı seed (değişmedi)
            const string adminRole = "Admin";
            if (!await _roles.RoleExistsAsync(adminRole))
                await _roles.CreateAsync(new IdentityRole<Guid>(adminRole));

            var user = await _users.FindByNameAsync("admin");
            if (user is null)
            {
                user = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@ikarus.local",
                    TenantId = tenant.Id,
                    EmailConfirmed = true
                };
                await _users.CreateAsync(user, "Pass123!");
                await _users.AddToRoleAsync(user, adminRole);
            }
        }
    }
}

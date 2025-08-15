using IKARUSWEB.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public sealed class Tenant : BaseEntity
    {
        public string Name { get; private set; } = null!;
        public string Country { get; private set; } = null!;
        public string City { get; private set; } = null!;
        public string Street { get; private set; } = null!;

        // 3 harf ISO kodu – Currency tablosuyla ilişki kuracağız
        public string? DefaultCurrencyCode { get; private set; } // nullable oldu

        public string TimeZone { get; private set; } = "Europe/Istanbul";
        public string DefaultCulture { get; private set; } = "tr-TR";

        //EF Core için boş constructor gerekli
        private Tenant() { }

        public Tenant(string name, string country, string city, string street,
                   string timeZone, string defaultCulture)
        {
            Name = name;
            Country = country;
            City = city;
            Street = street;
            TimeZone = timeZone;
            DefaultCulture = defaultCulture.Trim();
            // DefaultCurrencyCode burada boş kalacak; seed sonunda set edilecek
        }

        public Tenant Rename(string name) { Name = name; Touch(); return this; }

        public Tenant SetDefaultCurrency(string code)
        {
            var c = code.Trim().ToUpperInvariant();
            if (c.Length != 3) throw new ArgumentException("Currency code must be 3 letters.", nameof(code));
            DefaultCurrencyCode = c;
            Touch();
            return this;
        }
    }
}

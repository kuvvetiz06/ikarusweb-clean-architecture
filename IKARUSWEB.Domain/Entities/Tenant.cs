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
        // Kiracı/Otel kimliği
        public string Name { get; private set; } = string.Empty;

        // Adres ve meta
        public string Street { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;

        // Otel opsiyonları
        public string DefaultCurrency { get; private set; } = "TRY";
        public string TimeZone { get; private set; } = "Europe/Istanbul";

        // Çok dillilik için varsayılan kültür (UI tarafını yönlendirmek için)
        public string DefaultCulture { get; private set; } = "tr-TR";

        // İlişkiler
        private readonly List<Room> _rooms = new();
        public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

        // Factory-like ctor (setter’ları private tutuyoruz)
        public Tenant(string name, string country, string city, string street,
                      string defaultCurrency = "TRY", string timeZone = "Europe/Istanbul", string defaultCulture = "tr-TR")
        {
            Name = name.Trim();
            Country = country.Trim();
            City = city.Trim();
            Street = street.Trim();
            DefaultCurrency = defaultCurrency;
            TimeZone = timeZone;
            DefaultCulture = defaultCulture;
        }

        // For EF
        private Tenant() { }

        public void Rename(string newName)
        {
            Name = string.IsNullOrWhiteSpace(newName) ? Name : newName.Trim();
            Touch();
        }
    }
}

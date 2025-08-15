using IKARUSWEB.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public sealed class Currency : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; private set; }

        public string Name { get; private set; } = null!;
        public string Code { get; private set; } = null!; // ISO 4217 (3)
        public decimal CurrencyMultiplier { get; private set; } // > 0
        public decimal Rate { get; private set; }                 // ≥ 0

        private Currency() { }

        public Currency(Guid tenantId, string name, string code, decimal currencyMultiplier, decimal rate)
        : this(name, code, currencyMultiplier, rate)  // istersen mevcut ctor'u çağır
        {
            TenantId = tenantId;
        }
        public Currency(string name, string code, decimal currencyMultiplier, decimal rate)
        {
            Rename(name);
            SetCode(code);
            SetMultiplier(currencyMultiplier);
            SetRate(rate);
        }

        public Currency Rename(string name) { Name = name; Touch(); return this; }
        public Currency SetCode(string code)
        {
            var c = code.Trim().ToUpperInvariant();
            if (c.Length != 3) throw new ArgumentException("Code must be 3 letters.", nameof(code));
            Code = c; Touch(); return this;
        }
        public Currency SetMultiplier(decimal m)
        { if (m <= 0) throw new ArgumentOutOfRangeException(nameof(m)); CurrencyMultiplier = m; Touch(); return this; }
        public Currency SetRate(decimal r)
        { if (r < 0) throw new ArgumentOutOfRangeException(nameof(r)); Rate = r; Touch(); return this; }
    }
}

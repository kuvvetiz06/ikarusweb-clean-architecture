using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Mapping
{

    public sealed class CurrencyDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Code { get; init; } = default!;
        public decimal CurrencyMultiplier { get; init; }
        public decimal Rate { get; init; }
    }

}

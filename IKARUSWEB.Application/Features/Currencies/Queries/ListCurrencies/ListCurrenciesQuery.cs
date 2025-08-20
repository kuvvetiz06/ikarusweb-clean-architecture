using IKARUSWEB.Application.Mapping;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Currencies.Queries.ListCurrencies
{
    public sealed record ListCurrenciesQuery(string? Q = null) : IRequest<IReadOnlyList<CurrencyDto>>;

}

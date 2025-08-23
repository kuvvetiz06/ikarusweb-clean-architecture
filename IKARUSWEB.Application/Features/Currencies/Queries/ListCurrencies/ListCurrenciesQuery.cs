using IKARUSWEB.Application.Contracts.Currencies;
using MediatR;

namespace IKARUSWEB.Application.Features.Currencies.Queries.ListCurrencies
{
    public sealed record ListCurrenciesQuery(string? Q = null) : IRequest<IReadOnlyList<CurrencyDto>>;

}

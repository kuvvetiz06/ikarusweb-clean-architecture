using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Contracts.Currencies;
using MediatR;


namespace IKARUSWEB.Application.Features.Currencies.Commands.UpdateCurrencyRate
{
    public sealed record UpdateCurrencyRateCommand(Guid Id, decimal Rate) : IRequest<Result<CurrencyDto>>;
}

using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Contracts.Currencies;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Currencies.Commands.CreateCurrency
{
    public sealed record CreateCurrencyCommand(
    string Name,
    string Code,
    decimal CurrencyMultiplier,
    decimal Rate
) : IRequest<Result<CurrencyDto>>;
}

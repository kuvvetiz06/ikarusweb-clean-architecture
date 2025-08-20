using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Mapping;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Currencies.Commands.UpdateCurrencyRate
{
    public sealed record UpdateCurrencyRateCommand(Guid Id, decimal Rate) : IRequest<Result<CurrencyDto>>;
}

using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.Currencies.Commands.CreateCurrency;
using IKARUSWEB.Application.Features.Currencies.Commands.UpdateCurrencyRate;
using IKARUSWEB.Application.Features.Currencies.Queries.ListCurrencies;
using IKARUSWEB.Application.Contracts.Currencies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // JWT zorunlu
    public sealed class CurrenciesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurrenciesController(IMediator mediator) => _mediator = mediator;

        /// <summary>Create a new currency.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(Result<CurrencyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<CurrencyDto>>> Create([FromBody] CreateCurrencyCommand cmd, CancellationToken ct)
            => Ok(await _mediator.Send(cmd, ct));

        /// <summary>Update currency rate.</summary>
        [HttpPatch("{id:guid}/rate")]
        [ProducesResponseType(typeof(Result<CurrencyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<CurrencyDto>>> UpdateRate([FromRoute] Guid id, [FromBody] decimal rate, CancellationToken ct)
            => Ok(await _mediator.Send(new UpdateCurrencyRateCommand(id, rate), ct));

        /// <summary>List currencies (tenant scoped).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<CurrencyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<CurrencyDto>>> List([FromQuery] string? q, CancellationToken ct)
            => Ok(await _mediator.Send(new ListCurrenciesQuery(q), ct));
    }
}

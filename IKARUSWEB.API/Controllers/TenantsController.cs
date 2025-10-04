using IKARUSWEB.Application.Features.Tenants.Commands.CreateTenant;
using IKARUSWEB.Application.Features.Tenants.Dtos;
using IKARUSWEB.Application.Features.Tenants.Queries.GetTenantById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class TenantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TenantsController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<ActionResult<Result<TenantDto>>> Create([FromBody] CreateTenantCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (!result.Succeeded) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Result<TenantDto>>> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTenantByIdQuery(id), ct);
            if (!result.Succeeded) return NotFound(result);
            return Ok(result);
        }
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType;
using IKARUSWEB.Application.Features.RoomBedTypes.Commands.DeleteRoomBedType;
using IKARUSWEB.Application.Features.RoomBedTypes.Commands.UpdateRoomBedTypeName;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using IKARUSWEB.Application.Features.RoomBedTypes.Queries.GetRoomBedTypeById;
using IKARUSWEB.Application.Features.RoomBedTypes.Queries.ListRoomBedTypes;
using IKARUSWEB.Domain.Entities;
using IKARUSWEB.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IKARUSWEB.API.Controllers
{
    [ApiController]
    [Route("api/roombedtypes")]
    [Authorize]
    public sealed class RoomBedTypesController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly AppDbContext _db;
        private readonly ITenantProvider _tenant;
        private readonly IMapper _mapper;

        public RoomBedTypesController(ISender mediator, AppDbContext db, ITenantProvider tenant, IMapper mapper)
        { _mediator = mediator; _db = db; _tenant = tenant; _mapper = mapper; }

        // List (basit)
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RoomBedTypeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] string? q, CancellationToken ct)
            => Ok(await _mediator.Send(new ListRoomBedTypesQuery(q), ct));

        // Get
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RoomBedTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var dto = await _mediator.Send(new GetRoomBedTypeByIdQuery(id), ct);
            return dto is null ? NotFound(new { title = "Not Found" }) : Ok(dto);
        }

        // Create (Name, Code, Description)
        [HttpPost]
        [ProducesResponseType(typeof(RoomBedTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRoomBedTypeCommand cmd, CancellationToken ct)
        {
            var res = await _mediator.Send(cmd, ct);
            return res.Succeeded ? Ok(res.Data) : BadRequest(new { title = "Validation Error", detail = res.Message });
        }

        // Update Name (only)
        [HttpPut("{id:guid}/name")]
        [ProducesResponseType(typeof(RoomBedTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Rename(Guid id, [FromBody] UpdateRoomBedTypeNameCommand body, CancellationToken ct)
        {
            if (id != body.Id) return BadRequest(new { title = "Bad Request", detail = "Route id mismatch" });

            var res = await _mediator.Send(body, ct);
            if (res.Succeeded) return Ok(res.Data);

            var msg = res.Message ?? "Error";
            if (string.Equals(msg, "Not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { title = "Not Found" });

            return BadRequest(new { title = "Validation Error", detail = msg });
        }

        // Delete
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var res = await _mediator.Send(new DeleteRoomBedTypeCommand(id), ct);
            return res.Succeeded ? NoContent() :
                   StatusCode(404, new { title = "Not Found", detail = res.Message });
        }

        // DevExtreme server-side data
        [HttpGet("data")]
        public async Task<IActionResult> Data([FromQuery] DataSourceLoadOptions load, CancellationToken ct)
        {
            var query = _db.RoomBedTypes.AsNoTracking()
                .Where(x => x.TenantId == _tenant.TenantId)
                .OrderBy(x => x.Name)
                .Select(x => new RoomBedTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    
                });

            var result = await DataSourceLoader.LoadAsync(query, load, ct);
            return Ok(result);
        }
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using IKARUSWEB.API.Localization;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Localization;
using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType;
using IKARUSWEB.Application.Features.RoomBedTypes.Commands.DeleteRoomBedType;
using IKARUSWEB.Application.Features.RoomBedTypes.Commands.UpdateRoomBedType;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using IKARUSWEB.Application.Features.RoomBedTypes.Queries.GetRoomBedTypeById;
using IKARUSWEB.Application.Features.RoomBedTypes.Queries.ListRoomBedTypes;
using IKARUSWEB.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

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
        private readonly IStringLocalizer<CommonResource> _L;

        public RoomBedTypesController(ISender mediator, AppDbContext db, ITenantProvider tenant, IMapper mapper, IStringLocalizer<CommonResource> localizer)
        { _mediator = mediator; _db = db; _tenant = tenant; _mapper = mapper; _L = localizer; }

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
            return dto is null ? NotFound(Result<RoomBedTypeDto>.Failure(_L[MessageCodes.Common.RecordNotFound].Value)) : Ok(dto);
        }

        // Create (Name, Code, Description)
        [HttpPost]
        [ProducesResponseType(typeof(RoomBedTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRoomBedTypeCommand cmd, CancellationToken ct)
        {
            var res = await _mediator.Send(cmd, ct);
            return res.Succeeded ? Ok(Result<RoomBedTypeDto>
                    .Success(res.Data ?? new RoomBedTypeDto(), _L[MessageCodes.Common.RecordCreated].Value)) : BadRequest(Result<RoomBedTypeDto>.Failure(_L[MessageCodes.Common.Validation].Value));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(Result<RoomBedTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomBedTypeCommand body, CancellationToken ct)
        {

            if (id != body.Id)
                return BadRequest(Result<RoomBedTypeDto>.Failure(_L[MessageCodes.Common.RecordNotFound].Value));


            var cmd = new UpdateRoomBedTypeCommand(
                body.Id, _tenant.TenantId, body.Name, body.Code, body.Description, body.IsActive);

            var res = await _mediator.Send(cmd, ct);

            if (res.Succeeded)

                return Ok(Result<RoomBedTypeDto>
                    .Success(res.Data ?? new RoomBedTypeDto(), _L[MessageCodes.Common.RecordUpdated].Value));

            var msg = res.Message ?? "Error";
            if (string.Equals(msg, MessageCodes.Common.RecordNotFound, StringComparison.OrdinalIgnoreCase))
                return NotFound(Result<RoomBedTypeDto>.Failure(_L[MessageCodes.Common.RecordNotFound].Value));

            return BadRequest(Result<RoomBedTypeDto>.Failure(_L[MessageCodes.Common.BadRequest].Value));

        }
      
        // Delete
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var res = await _mediator.Send(new DeleteRoomBedTypeCommand(id), ct);
            return res.Succeeded ? NoContent() :
                   StatusCode(404, Result<RoomBedTypeDto>.Failure(_L[MessageCodes.Common.RecordNotFound].Value));
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

using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using IKARUSWEB.API.Infrastructure.DevExtreme;
using IKARUSWEB.API.Localization;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Localization;
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
        public async Task<IActionResult> List([FromQuery] string? q, CancellationToken ct)
            => Ok(await _mediator.Send(new ListRoomBedTypesQuery(q), ct));

        // Get
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var dto = await _mediator.Send(new GetRoomBedTypeByIdQuery(id), ct);
            return dto is null ? NotFound(Result<RoomBedTypeDto>.Failure(_L[MessageCodes.Common.RecordNotFound].Value)) : Ok(dto);
        }

        // Create (Name, Code, Description)
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateRoomBedTypeCommand cmd, CancellationToken ct)

                   => await _mediator.Send(cmd, ct);



        [HttpPut("{id:guid}")]
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
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var res = await _mediator.Send(new DeleteRoomBedTypeCommand(id), ct);
            return res.Succeeded ? NoContent() :
                   StatusCode(404, Result<RoomBedTypeDto>.Failure(_L[MessageCodes.Common.RecordNotFound].Value));
        }

        // DevExtreme server-side data
        [HttpGet("data")]
        [DefaultSort(nameof(RoomBedTypeDto.CreatedAt), true)]
        public async Task<IActionResult> Data([FromQuery] DataSourceLoadOptions load, CancellationToken ct)
        {

            var query = _db.RoomBedTypes.AsNoTracking()
                .Where(x => x.TenantId == _tenant.TenantId)
                .Select(x => new RoomBedTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,

                });

            var result = await DataSourceLoader.LoadAsync(query, load, ct);
            return Ok(result);
        }
    }
}

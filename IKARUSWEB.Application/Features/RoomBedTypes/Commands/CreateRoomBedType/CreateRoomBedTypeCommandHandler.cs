using AutoMapper;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Localization;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using IKARUSWEB.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType
{
    public sealed class CreateRoomBedTypeCommandHandler
     : IRequestHandler<CreateRoomBedTypeCommand, Result<RoomBedTypeDto>>
    {
        private readonly ITenantProvider _tenant;
        private readonly IMapper _mapper;
        private readonly IRoomBedTypeWriteRepository _write;
        public CreateRoomBedTypeCommandHandler(ITenantProvider tenant, IRoomBedTypeWriteRepository write, IMapper mapper)
        { _tenant = tenant; _write = write; _mapper = mapper; }

        public async Task<Result<RoomBedTypeDto>> Handle(CreateRoomBedTypeCommand request, CancellationToken ct)
        {
            var entity = new RoomBedType((Guid)_tenant.TenantId, request.Name,
                request.Code?.Trim()?.ToUpperInvariant(), request.Description);

            await _write.CreateAsync(entity, ct);

            var dto = _mapper.Map<RoomBedTypeDto>(entity);
            return Result<RoomBedTypeDto>.Success(dto, MessageCodes.Common.RecordCreated);
        }
    }
}

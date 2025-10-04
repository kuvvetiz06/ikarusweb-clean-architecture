using AutoMapper;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Localization;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using IKARUSWEB.Application.Features.RoomBedTypes.Logging;
using IKARUSWEB.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType
{
    public sealed class CreateRoomBedTypeCommandHandler
     : IRequestHandler<CreateRoomBedTypeCommand, Result<RoomBedTypeDto>>
    {
        private readonly ITenantProvider _tenant;
        private readonly IMapper _mapper;
        private readonly IRoomBedTypeWriteRepository _write;
        private readonly ILogger<CreateRoomBedTypeCommandHandler> _logger;
        public CreateRoomBedTypeCommandHandler(ITenantProvider tenant,
            IRoomBedTypeWriteRepository write,
            IMapper mapper,
            ILogger<CreateRoomBedTypeCommandHandler> logger)
        { _tenant = tenant; _write = write; _mapper = mapper; _logger = logger; }

        public async Task<Result<RoomBedTypeDto>> Handle(CreateRoomBedTypeCommand request, CancellationToken ct)
        {
            var entity = new RoomBedType((Guid)_tenant.TenantId, request.Name,
                request.Code?.Trim()?.ToUpperInvariant(), request.Description);

            try
            {
                await _write.CreateAsync(entity, ct);

                _logger.RoomBedTypeAdded(entity.Id);
            }
            catch (Exception ex)
            {

                _logger.RoomBedTypeNotAdded("Unexpected error", ex);

                return Result<RoomBedTypeDto>.Failure(MessageCodes.Common.UnExpectedError);
            }


            var dto = _mapper.Map<RoomBedTypeDto>(entity);
            return Result<RoomBedTypeDto>.Created(dto, MessageCodes.Common.RecordCreated);
        }
    }
}

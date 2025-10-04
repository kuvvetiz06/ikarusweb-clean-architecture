using AutoMapper;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using IKARUSWEB.Application.Features.RoomBedTypes.Logging;
using MediatR;
using Microsoft.Extensions.Logging;


namespace IKARUSWEB.Application.Features.RoomBedTypes.Queries.GetRoomBedTypeById
{
    public sealed class GetRoomBedTypeByIdQueryHandler
    : IRequestHandler<GetRoomBedTypeByIdQuery, Result<RoomBedTypeDto?>>
    {
        private readonly IRoomBedTypeReadRepository _read;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRoomBedTypeByIdQueryHandler> _logger;
        public GetRoomBedTypeByIdQueryHandler(IRoomBedTypeReadRepository read,
            IMapper mapper,
            ILogger<GetRoomBedTypeByIdQueryHandler> logger)
        { _read = read; _mapper = mapper; _logger = logger; }

        public async Task<Result<RoomBedTypeDto?>> Handle(GetRoomBedTypeByIdQuery request, CancellationToken ct)
        {
            var entity = await _read.GetByIdAsync(request.Id, ct);
            if (entity is null)
            {
                _logger.RoomBedTypeNotRetrieved(entity.Id);
                return Result<RoomBedTypeDto?>.NotFound();
            }
            var dto = _mapper.Map<RoomBedTypeDto>(entity);

            _logger.LogInformation("RoomBedType get successfully with TenantId {TenantId} - Id {Id}", entity.TenantId, entity.Id);
            return Result<RoomBedTypeDto?>.Success(dto);

        }
    }
}

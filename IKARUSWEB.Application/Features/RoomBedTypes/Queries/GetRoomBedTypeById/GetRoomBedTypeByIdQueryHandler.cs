using AutoMapper;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;


namespace IKARUSWEB.Application.Features.RoomBedTypes.Queries.GetRoomBedTypeById
{
    public sealed class GetRoomBedTypeByIdQueryHandler
    : IRequestHandler<GetRoomBedTypeByIdQuery, RoomBedTypeDto?>
    {
        private readonly IRoomBedTypeReadRepository _read;
        private readonly IMapper _mapper;

        public GetRoomBedTypeByIdQueryHandler(IRoomBedTypeReadRepository read, IMapper mapper)
        { _read = read; _mapper = mapper; }

        public async Task<RoomBedTypeDto?> Handle(GetRoomBedTypeByIdQuery request, CancellationToken ct)
        {
            var entity = await _read.GetByIdAsync(request.Id, ct);
            return entity is null ? null : _mapper.Map<RoomBedTypeDto>(entity);
        }
    }
}

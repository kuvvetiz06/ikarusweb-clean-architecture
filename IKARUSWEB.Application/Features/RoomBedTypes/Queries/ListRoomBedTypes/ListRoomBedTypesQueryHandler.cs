using AutoMapper;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Queries.ListRoomBedTypes
{
    public sealed class ListRoomBedTypesQueryHandler
    : IRequestHandler<ListRoomBedTypesQuery, IReadOnlyList<RoomBedTypeDto>>
    {
        private readonly IRoomBedTypeReadRepository _read;
        private readonly IMapper _mapper;

        public ListRoomBedTypesQueryHandler(IRoomBedTypeReadRepository read, IMapper mapper)
        { _read = read; _mapper = mapper; }

        public async Task<IReadOnlyList<RoomBedTypeDto>> Handle(ListRoomBedTypesQuery request, CancellationToken ct)
        {
            var list = await _read.ListAsync(request.Q, ct);
            return _mapper.Map<IReadOnlyList<RoomBedTypeDto>>(list);
        }
    }
}

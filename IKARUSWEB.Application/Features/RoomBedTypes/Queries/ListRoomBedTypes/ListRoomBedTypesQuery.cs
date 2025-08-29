using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Queries.ListRoomBedTypes
{
    public sealed record ListRoomBedTypesQuery(string? Q) : IRequest<IReadOnlyList<RoomBedTypeDto>>;
}

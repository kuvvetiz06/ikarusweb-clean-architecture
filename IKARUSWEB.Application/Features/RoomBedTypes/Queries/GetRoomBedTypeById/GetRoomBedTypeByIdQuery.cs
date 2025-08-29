using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Queries.GetRoomBedTypeById
{
    public sealed record GetRoomBedTypeByIdQuery(Guid Id) : IRequest<RoomBedTypeDto?>;
}

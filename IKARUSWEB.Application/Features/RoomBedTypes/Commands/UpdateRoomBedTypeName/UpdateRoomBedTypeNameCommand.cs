using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.UpdateRoomBedTypeName
{
    public sealed record UpdateRoomBedTypeNameCommand(Guid Id, string Name) : IRequest<Result<RoomBedTypeDto>>;
}

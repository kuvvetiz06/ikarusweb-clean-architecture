using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType
{
    public sealed record CreateRoomBedTypeCommand(string Name, string? Code, string? Description) : IRequest<Result<RoomBedTypeDto>>;
}

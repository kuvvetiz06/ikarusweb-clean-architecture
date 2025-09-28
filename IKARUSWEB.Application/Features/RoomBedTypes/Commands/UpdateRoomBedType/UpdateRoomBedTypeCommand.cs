using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.UpdateRoomBedType
{
    public sealed record UpdateRoomBedTypeCommand(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Code,
    string? Description,
    bool IsActive
) : IRequest<Result<RoomBedTypeDto>>;
}

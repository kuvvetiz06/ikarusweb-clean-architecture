using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;


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

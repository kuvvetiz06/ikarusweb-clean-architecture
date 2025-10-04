using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;


namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType
{
    public sealed record CreateRoomBedTypeCommand(string Name, string? Code, string? Description) : IRequest<Result<RoomBedTypeDto>>;
}

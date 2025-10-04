using MediatR;


namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.DeleteRoomBedType
{
    public sealed record DeleteRoomBedTypeCommand(Guid Id) : IRequest<Result<Unit>>;
}

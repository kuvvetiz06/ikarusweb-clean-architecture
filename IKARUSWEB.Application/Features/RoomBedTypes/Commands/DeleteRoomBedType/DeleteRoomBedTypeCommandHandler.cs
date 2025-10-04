using IKARUSWEB.Application.Abstractions.Localization;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using MediatR;


namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.DeleteRoomBedType
{
    public sealed class DeleteRoomBedTypeCommandHandler
     : IRequestHandler<DeleteRoomBedTypeCommand, Result<Unit>>
    {
        private readonly IRoomBedTypeReadRepository _read;
        private readonly IRoomBedTypeWriteRepository _write;

        public DeleteRoomBedTypeCommandHandler(IRoomBedTypeReadRepository read, IRoomBedTypeWriteRepository write)
        { _read = read; _write = write; }

        public async Task<Result<Unit>> Handle(DeleteRoomBedTypeCommand request, CancellationToken ct)
        {
            var entity = await _read.GetByIdAsync(request.Id, ct);
            if (entity is null) return Result<Unit>.Failure(MessageCodes.Common.RecordDeleted);

            await _write.DeleteAsync(entity, ct);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}

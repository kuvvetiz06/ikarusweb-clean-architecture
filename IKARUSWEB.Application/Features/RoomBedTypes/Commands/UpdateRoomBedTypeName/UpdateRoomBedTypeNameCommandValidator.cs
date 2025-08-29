using FluentValidation;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;


namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.UpdateRoomBedTypeName
{
    public sealed class UpdateRoomBedTypeNameCommandValidator : AbstractValidator<UpdateRoomBedTypeNameCommand>
    {
        public UpdateRoomBedTypeNameCommandValidator(IRoomBedTypeReadRepository read)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

            RuleFor(x => x).MustAsync(async (cmd, ct) =>
            {
                // Aynı isim başka kayıtta var mı?
                var exists = await read.ExistsByNameAsync(cmd.Name, ct);
                // Burada "var ama aynı Id mi?" kontrolünü handler yapacağız.
                return true;
            }).WithMessage("Name already exists.");
        }
    }
}

using FluentValidation;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;


namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType
{
    public sealed class CreateRoomBedTypeCommandValidator : AbstractValidator<CreateRoomBedTypeCommand>
    {
        public CreateRoomBedTypeCommandValidator(IRoomBedTypeReadRepository read)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("RoomBedType.Name.Required").MaximumLength(25).WithMessage("RoomBedType.Name.MaxLength||{MaxLength}");

            RuleFor(x => x.Code).NotEmpty().WithMessage("RoomBedType.Code.Required").MaximumLength(8).WithMessage("RoomBedType.Code.MaxLength");

            RuleFor(x => x.Description).MaximumLength(150).WithMessage("RoomBedType.Description.MaxLength"); ;

            RuleFor(x => x.Name)
                .MustAsync(async (name, ct) => !await read.ExistsByNameAsync(name, excludeId: null, ct))
                .WithMessage("RoomBedType.Name.Exits");

            RuleFor(x => x.Code)
                .MustAsync(async (code, ct) =>
                    string.IsNullOrWhiteSpace(code) || !await read.ExistsByCodeAsync(code.Trim().ToUpperInvariant(), ct))
                .WithMessage("RoomBedType.Code.Exits");
        }
    }
}

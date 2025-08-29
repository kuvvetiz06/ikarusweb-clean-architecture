using FluentValidation;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;


namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.CreateRoomBedType
{
    public sealed class CreateRoomBedTypeCommandValidator : AbstractValidator<CreateRoomBedTypeCommand>
    {
        public CreateRoomBedTypeCommandValidator(IRoomBedTypeReadRepository read)
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

            RuleFor(x => x.Code)
                .MaximumLength(32)
                .Matches("^[A-Za-z0-9_\\-]*$").When(x => !string.IsNullOrWhiteSpace(x.Code))
                .WithMessage("Code allows letters, digits, '-' and '_'.");

            RuleFor(x => x.Description).MaximumLength(500);

            RuleFor(x => x.Name)
                .MustAsync(async (name, ct) => !await read.ExistsByNameAsync(name, ct))
                .WithMessage("Name already exists.");

            RuleFor(x => x.Code)
                .MustAsync(async (code, ct) =>
                    string.IsNullOrWhiteSpace(code) || !await read.ExistsByCodeAsync(code.Trim().ToUpperInvariant(), ct))
                .WithMessage("Code already exists.");
        }
    }
}

using FluentValidation;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Repositories;



namespace IKARUSWEB.Application.Features.Currencies.Commands.CreateCurrency
{
    public sealed class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
    {
       
        public CreateCurrencyCommandValidator(ICurrencyRepository repo)
        {
        

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Currency name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.");

            RuleFor(x => x.CurrencyMultiplier)
                .GreaterThan(0).WithMessage("Multiplier must be greater than 0.");

            RuleFor(x => x.Rate)
                .GreaterThanOrEqualTo(0).WithMessage("Rate must be greater or equal to 0.");

            RuleFor(x => x.Code)
                .MustAsync(async (code, ct) => !await repo.ExistsByCodeAsync(code, ct))
                .WithMessage("A currency with the same code already exists.");

            RuleFor(x => x.Name)
                .MustAsync(async (name, ct) => !await repo.ExistsByNameAsync(name, ct))
                .WithMessage("A currency with the same name already exists.");
        }
    }
}

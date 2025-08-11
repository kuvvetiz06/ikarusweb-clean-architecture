using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Tenants.Commands.CreateTenant
{
    public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
    {
        public CreateTenantCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(200);
            RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DefaultCurrency).NotEmpty().Length(3); // ISO 4217
            RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DefaultCulture).NotEmpty().MaximumLength(10); // tr-TR, en-US...
        }
    }
}

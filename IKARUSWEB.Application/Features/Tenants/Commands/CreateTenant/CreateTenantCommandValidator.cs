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
            RuleFor(x => x.Name)
                .NotEmpty().WithErrorCode("Validation.Tenant.Name.Required")
                .MinimumLength(2).WithErrorCode("Validation.Tenant.Name.MinLength")
                .MaximumLength(200).WithErrorCode("Validation.Tenant.Name.MaxLength");

            RuleFor(x => x.Country).NotEmpty().WithErrorCode("Validation.Tenant.Country.Required");
            RuleFor(x => x.City).NotEmpty().WithErrorCode("Validation.Tenant.City.Required");
            RuleFor(x => x.Street).NotEmpty().WithErrorCode("Validation.Tenant.Street.Required");
            RuleFor(x => x.DefaultCurrency).NotEmpty().Length(3).WithErrorCode("Validation.Tenant.Currency.Length3");
            RuleFor(x => x.TimeZone).NotEmpty().WithErrorCode("Validation.Tenant.TimeZone.Required");
            RuleFor(x => x.DefaultCulture).NotEmpty().WithErrorCode("Validation.Tenant.DefaultCulture.Required");
        }
    }
}

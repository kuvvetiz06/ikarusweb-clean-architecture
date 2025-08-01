using FluentValidation;
using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Validators
{
    public class TenantValidator : AbstractValidator<Tenant>
    {
        public TenantValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tenant name is required")
                .MaximumLength(100);

            RuleFor(x => x.TenantIdentifier)
                .NotEmpty().WithMessage("Tenant identifier is required");
        }
    }
}

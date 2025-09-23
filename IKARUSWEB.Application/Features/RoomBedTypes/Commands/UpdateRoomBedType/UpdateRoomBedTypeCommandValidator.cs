using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.UpdateRoomBedType
{
    public sealed class UpdateRoomBedTypeCommandValidator : AbstractValidator<UpdateRoomBedTypeCommand>
    {
        public UpdateRoomBedTypeCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.TenantId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).MaximumLength(50);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }
}

using IKARUSWEB.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.DeleteRoomBedType
{
    public sealed record DeleteRoomBedTypeCommand(Guid Id) : IRequest<Result<Unit>>;
}

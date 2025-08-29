using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Dtos
{
    public sealed class RoomBedTypeDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string? Code { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; }
    }
}

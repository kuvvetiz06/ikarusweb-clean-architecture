using IKARUSWEB.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public sealed class Room : BaseEntity
    {
        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = default!;

        public string Number { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        public Guid RoomTypeId { get; private set; }
        public RoomType RoomType { get; private set; } = default!;

        public Guid RoomViewId { get; private set; }
        public RoomView RoomView { get; private set; } = default!;

        public Guid RoomLocationId { get; private set; }
        public RoomLocation RoomLocation { get; private set; } = default!;

        public Guid RoomBedTypeId { get; private set; }
        public RoomBedType RoomBedType { get; private set; } = default!;

        public string Floor { get; private set; } = string.Empty;
        public int MaxBed { get; private set; }

        private Room() { }

        public Room(Guid tenantId, string number, Guid typeId, Guid viewId, Guid locationId, Guid bedTypeId,
                    string? description = null, string? floor = null, int maxBed = 1)
        {
            TenantId = tenantId;
            Number = number.Trim();
            RoomTypeId = typeId;
            RoomViewId = viewId;
            RoomLocationId = locationId;
            RoomBedTypeId = bedTypeId;
            Description = description?.Trim() ?? string.Empty;
            Floor = floor?.Trim() ?? string.Empty;
            MaxBed = Math.Max(maxBed, 1);
        }
    }
}

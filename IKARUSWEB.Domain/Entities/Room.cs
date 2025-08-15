using IKARUSWEB.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public sealed class Room : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; private set; }

        // Detay alanlar
        public string Number { get; private set; } = null!;  // req, max 50
        public string? Description { get; private set; }     // max 500
        public string? Floor { get; private set; }           // max 20
        public int MaxBed { get; private set; }              // req, >= 1

        // İlişkiler (zorunlu)
        public Guid RoomTypeId { get; private set; }
        public Guid RoomViewId { get; private set; }
        public Guid RoomBedTypeId { get; private set; }
        public Guid RoomLocationId { get; private set; }

        // Navigations
        public RoomType RoomType { get; private set; } = default!;
        public RoomView RoomView { get; private set; } = default!;
        public RoomBedType RoomBedType { get; private set; } = default!;
        public RoomLocation RoomLocation { get; private set; } = default!;

        private Room() { }
        public Room(string number, int maxBed, Guid roomTypeId, Guid roomViewId, Guid roomBedTypeId, Guid roomLocationId, string? description = null, string? floor = null)
        {
            Number = number.Trim();
            MaxBed = maxBed;
            RoomTypeId = roomTypeId;
            RoomViewId = roomViewId;
            RoomBedTypeId = roomBedTypeId;
            RoomLocationId = roomLocationId;
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Floor = string.IsNullOrWhiteSpace(floor) ? null : floor.Trim();
        }
    }
}

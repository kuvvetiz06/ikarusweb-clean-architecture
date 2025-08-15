using IKARUSWEB.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public sealed class RoomType : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Code { get; private set; }
        public string? Description { get; private set; }

        private readonly List<Room> _rooms = new();
        public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

        private RoomType() { }
        public RoomType(string name, string? code = null, string? description = null)
        { Name = name; Code = code?.Trim(); Description = description; }

        public RoomType Rename(string name) { Name = name; Touch(); return this; }
    }
}

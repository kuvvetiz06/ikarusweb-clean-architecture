using IKARUSWEB.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public sealed class RoomBedType : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        private RoomBedType() { }
        public RoomBedType(string name)
        {
            Name = name.Trim();
        }
    }

}

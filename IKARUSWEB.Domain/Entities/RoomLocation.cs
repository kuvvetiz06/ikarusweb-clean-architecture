using IKARUSWEB.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public sealed class RoomLocation : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        private RoomLocation() { }
        public RoomLocation(string name)
        {
            Name = name.Trim();
        }
    }

}

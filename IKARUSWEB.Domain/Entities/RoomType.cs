using IKARUSWEB.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public sealed class RoomType : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        private RoomType() { }
        public RoomType(string name)
        {
            Name = name.Trim();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Abstractions
{
    public interface IDateTime
    {
        DateTime UtcNow { get; }
    }
}

using IKARUSWEB.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Entities
{
    public class RoomType : MultiTenantEntity
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;        
        public int Quantity { get; set; }                          
        public int BedCount { get; set; }                          
        public int MaxOccupancy { get; set; }                      
        public int MaxChildrenCount { get; set; }                  
        public int MaxInfantCount { get; set; }                    
        public string? Features { get; set; }
        public string? Description { get; set; }
       
    }
}

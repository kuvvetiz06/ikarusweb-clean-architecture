using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Abstractions
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public bool IsActive { get; protected set; } = true;
        public bool IsDeleted { get; protected set; } = false;

        public string? CreatedBy { get; protected set; }
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        public string? ModifiedBy { get; protected set; }
        public DateTime? ModifiedAt { get; protected set; }

        public string? DeletedBy { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }

        public void MarkCreated(string? by = null, DateTime? now = null)
        {
            IsActive = true;
            IsDeleted = false;
            CreatedBy = by;
            CreatedAt = now ?? DateTime.UtcNow;
        }

        public void MarkPassive(string? by = null, DateTime? now = null)
        {                 
            IsActive = false;
            ModifiedBy = by;
            ModifiedAt = now ?? DateTime.UtcNow;
        }

        public void Touch(string? by = null, DateTime? now = null)
        {
            ModifiedBy = by;
            ModifiedAt = now ?? DateTime.UtcNow;
        }

        public void MarkDeleted(string? by = null, DateTime? now = null)
        {
            if (IsDeleted) return;
            IsDeleted = true;
            IsActive = false;
            DeletedBy = by;
            DeletedAt = now ?? DateTime.UtcNow;
        }
    }

}

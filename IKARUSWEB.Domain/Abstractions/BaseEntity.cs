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

        // Soft-delete & audit
        public bool IsActive { get; protected set; } = true;
        public bool IsDeleted { get; protected set; } = false;

        public string? CreatedBy { get; protected set; }
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        public string? ModifiedBy { get; protected set; }
        public DateTime? ModifiedAt { get; protected set; }

        public string? DeletedBy { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }

        public void MarkDeleted(string? by = null)
        {
            if (IsDeleted) return;
            IsDeleted = true;
            IsActive = false;
            DeletedBy = by;
            DeletedAt = DateTime.UtcNow;
        }

        public void Touch(string? by = null)
        {
            ModifiedBy = by;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}

using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Persistence.Interceptors
{
    public sealed class TenantAssignmentInterceptor : SaveChangesInterceptor
    {
        private readonly ITenantProvider _tenant;
        public TenantAssignmentInterceptor(ITenantProvider tenant) => _tenant = tenant;

        public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> r)
        {
            AssignTenant(e.Context);
            return base.SavingChanges(e, r);
        }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e, InterceptionResult<int> r, CancellationToken ct = default)
        {
            AssignTenant(e.Context);
            return base.SavingChangesAsync(e, r, ct);
        }

        private void AssignTenant(DbContext? ctx)
        {
            if (ctx is null) return;
            var tid = _tenant.TenantId;
            if (tid is null) return;

            foreach (var entry in ctx.ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added) continue;
                if (entry.Entity is IMustHaveTenant t)
                {
                    var prop = entry.Property(nameof(IMustHaveTenant.TenantId));
                    if (prop.CurrentValue is Guid g && g != Guid.Empty) continue;
                    prop.CurrentValue = tid.Value;
                }
            }
        }
    }
}

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

        public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> result)
        {
            AssignTenantIfNeeded(e.Context);
            return base.SavingChanges(e, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
        {
            AssignTenantIfNeeded(e.Context);
            return base.SavingChangesAsync(e, result, ct);
        }

        private void AssignTenantIfNeeded(DbContext? ctx)
        {
            if (ctx is null) return;

            // Tenant çözülmemişse (login/anonim) hiç dokunma
            if (!_tenant.IsResolved) return;

            var tid = _tenant.TenantId;

            foreach (var entry in ctx.ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added) continue;

                if (entry.Entity is IMustHaveTenant)
                {
                    var prop = entry.Property(nameof(IMustHaveTenant.TenantId));

                    // EF, prop.CurrentValue null dönebilir; güvenli çekelim
                    var current = prop.CurrentValue is Guid g ? g : Guid.Empty;

                    if (current == Guid.Empty)
                    {
                        // Boşsa mevcut tenant ile doldur
                        prop.CurrentValue = tid;
                    }
                    else if (current != tid)
                    {
                        // Yanlış tenant ile ekleme girişimini engelle
                        throw new InvalidOperationException("Cross-tenant insert is not allowed.");
                    }
                }
            }
        }
    }

}

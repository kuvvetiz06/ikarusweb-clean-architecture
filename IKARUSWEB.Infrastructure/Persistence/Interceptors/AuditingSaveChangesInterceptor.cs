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
    public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IDateTime _clock;
        private readonly ICurrentUser _currentUser;

        public AuditingSaveChangesInterceptor(IDateTime clock, ICurrentUser currentUser)
        {
            _clock = clock;
            _currentUser = currentUser;
        }

        private void ApplyAudit(DbContext? ctx)
        {
            if (ctx is null) return;

            foreach (var entry in ctx.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.MarkCreated(_currentUser.UserName, _clock.UtcNow);
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Touch(_currentUser.UserName, _clock.UtcNow);
                }
            }
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}

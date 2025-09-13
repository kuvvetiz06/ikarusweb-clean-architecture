using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Domain.Security
{
    public sealed class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public Guid TenantId { get; private set; }
        public string Token { get; private set; } = default!; // random, tercihen hash'lenmiş tutulur
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? RevokedAt { get; private set; }
        public string? ReplacedByToken { get; private set; } // rotate edilen yeni token

        private RefreshToken() { }
        public RefreshToken(Guid userId, Guid tenantId, string token, DateTimeOffset expiresAt)
        { UserId = userId; TenantId = tenantId; Token = token; ExpiresAt = expiresAt; }

        public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

        public void Revoke() => RevokedAt ??= DateTimeOffset.UtcNow;
        public void RotateTo(string newToken)
        { Revoke(); ReplacedByToken = newToken; }
    }
}

using IKARUSWEB.Application.Common.Security;
using IKARUSWEB.Domain.Security;


namespace IKARUSWEB.Application.Abstractions.Security
{
    public interface ITokenService
    {
        (string token, DateTimeOffset expiresAt) Create(UserTicket user);
        Task<(string refreshToken, DateTimeOffset expiresAt)> IssueRefreshAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
        Task<(bool ok, RefreshToken? token)> ValidateRefreshAsync(string token, CancellationToken ct = default);
        Task<(string newToken, DateTimeOffset newExp)> RotateRefreshAsync(RefreshToken current, CancellationToken ct = default);
        Task RevokeRefreshAsync(string token, CancellationToken ct = default);
    }
}

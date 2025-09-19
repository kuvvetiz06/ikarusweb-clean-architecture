using IKARUSWEB.Application.Abstractions.Security;
using IKARUSWEB.Application.Common.Security;
using IKARUSWEB.Domain.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using IKARUSWEB.Infrastructure.Persistence;


namespace IKARUSWEB.Infrastructure.Auth
{
    public sealed class JwtTokenService : ITokenService
    {
        private readonly TokenOptions _opt;
        private readonly AppDbContext _db;
        public JwtTokenService(IOptions<TokenOptions> opt, AppDbContext db)
        {
            _opt = opt.Value; _db = db;
        }

        public (string token, DateTimeOffset expiresAt) Create(UserTicket user)
        {
            var now = DateTime.UtcNow;
            var exp = now.AddMinutes(_opt.AccessTokenMinutes);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new("tenant_id", user.TenantId?.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
            if (user.Roles is { Count: > 0 })
            {
                foreach (var r in user.Roles!)
                    if (!string.IsNullOrWhiteSpace(r))
                        claims.Add(new(ClaimTypes.Role, r));
            }
            if (!string.IsNullOrWhiteSpace(user.TenantName))
                claims.Add(new("tenant_name", user.TenantName));
            if (!string.IsNullOrWhiteSpace(user.FullName))
                claims.Add(new("full_name", user.FullName));


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: now,
                expires: exp,
                signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return (token, new DateTimeOffset(exp, TimeSpan.Zero));
        }

        public async Task<(string refreshToken, DateTimeOffset expiresAt)> IssueRefreshAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(bytes); // pratik; prod'da hashleyerek saklayabilirsin
            var exp = DateTimeOffset.UtcNow.AddDays(_opt.RefreshTokenDays);

            var rt = new RefreshToken(userId, tenantId, token, exp);
            _db.RefreshTokens.Add(rt);
            await _db.SaveChangesAsync(ct);

            return (token, exp);
        }

        public async Task<(bool ok, RefreshToken? token)> ValidateRefreshAsync(string token, CancellationToken ct = default)
        {
            var rt = await _db.RefreshTokens.AsTracking()
                .FirstOrDefaultAsync(x => x.Token == token, ct);

            if (rt is null || !rt.IsActive) return (false, null);
            return (true, rt);
        }

        public async Task<(string newToken, DateTimeOffset newExp)> RotateRefreshAsync(RefreshToken current, CancellationToken ct = default)
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var newToken = Convert.ToBase64String(bytes);
            var newExp = DateTimeOffset.UtcNow.AddDays(_opt.RefreshTokenDays);

            current.RotateTo(newToken);
            var rtNew = new RefreshToken(current.UserId, current.TenantId, newToken, newExp);
            _db.RefreshTokens.Add(rtNew);
            await _db.SaveChangesAsync(ct);

            return (newToken, newExp);
        }

        public async Task RevokeRefreshAsync(string token, CancellationToken ct = default)
        {
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, ct);
            if (rt is not null && rt.RevokedAt is null)
            {
                rt.Revoke();
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}

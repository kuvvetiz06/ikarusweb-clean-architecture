using IKARUSWEB.Application.Abstractions.Security;
using IKARUSWEB.Application.Common.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IKARUSWEB.Infrastructure.Auth
{
    public sealed class JwtTokenService : ITokenService
    {
        private readonly TokenOptions _opt;
        public JwtTokenService(IOptions<TokenOptions> opt) => _opt = opt.Value;

        public (string token, DateTimeOffset expiresAt) Create(UserTicket user)
        {
            var now = DateTime.UtcNow;
            var exp = now.AddMinutes(_opt.AccessTokenMinutes);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new("tenant_id", user.TenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
            foreach (var r in user.Roles) claims.Add(new(ClaimTypes.Role, r));

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
    }
}

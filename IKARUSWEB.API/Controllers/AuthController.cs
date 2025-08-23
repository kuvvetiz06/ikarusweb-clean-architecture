using IKARUSWEB.Application.Abstractions.Security;
using IKARUSWEB.Application.Common.Security;
using IKARUSWEB.Infrastructure.Auth;
using IKARUSWEB.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IKARUSWEB.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _users;
        private readonly ITokenService _tokens;

        public AuthController(UserManager<AppUser> users, ITokenService tokens)
        { _users = users; _tokens = tokens; }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _users.FindByNameAsync(req.UserName);
            if (user is null || !await _users.CheckPasswordAsync(user, req.Password))
                return Unauthorized(new { title = "Unauthorized", detail = "Geçersiz kimlik bilgileri." });

            var roles = await _users.GetRolesAsync(user);

            var ticket = new UserTicket(
                user.Id,
                user.TenantId,          // AppUser’da var
                user.UserName ?? "",
                roles);

            var (token, expiresAt) = _tokens.Create(ticket);
            return Ok(new AuthResponseDto(new AccessTokenDto(token, expiresAt)));
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout() => NoContent();
    }

    public sealed record LoginRequest(string UserName, string Password);
    public sealed record AuthResponseDto(AccessTokenDto Data);
    public sealed record AccessTokenDto(string AccessToken, DateTimeOffset ExpiresAt);
}

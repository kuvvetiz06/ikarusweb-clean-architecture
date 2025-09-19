using IKARUSWEB.Application.Abstractions.Security;
using IKARUSWEB.Application.Common.Security;
using IKARUSWEB.Application.Features.Tenants.Queries.GetTenantById;
using IKARUSWEB.Infrastructure.Auth;
using IKARUSWEB.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _users;
        private readonly ITokenService _tokens;
        private readonly IMediator _mediator;

        private void SetRefreshCookie(HttpContext http, string token, DateTimeOffset exp)
        {
            http.Response.Cookies.Append("refresh_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = http.Request.IsHttps, 
                SameSite = SameSiteMode.Lax,
                Expires = exp,
                Path = "/api/auth",

            });
        }
        private static void ClearRefreshCookie(HttpContext http)
        {
            http.Response.Cookies.Append("refresh_token", "",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = http.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UnixEpoch,
                    Path = "/api/auth"
                });
        }
        public AuthController(UserManager<AppUser> users, ITokenService tokens,IMediator mediator)
        { _users = users; _tokens = tokens; _mediator = mediator; }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var user = await _users.FindByNameAsync(req.UserName.Trim());
            if (user is null || !await _users.CheckPasswordAsync(user, req.Password.Trim()))
                return Unauthorized(new { title = "Unauthorized", detail = "Geçersiz kimlik bilgileri." });
            if (user.TenantCode != req.TenantCode.Trim())
                return Unauthorized(new { title = "Unauthorized", detail = "Geçersiz kimlik bilgileri." });

            var roles = await _users.GetRolesAsync(user);

            var tenant = await _mediator.Send(new GetTenantByIdQuery((Guid)user.TenantId), ct);
            if (tenant?.Data is null) 
                return Unauthorized(new { title = "Unauthorized", detail = "Geçersiz kimlik bilgileri." });


            var ticket = new UserTicket(user.Id, user.TenantId, user.UserName ?? "", roles, tenant.Data.Name, user.FullName);
         
            var (access, accessExp) = _tokens.Create(ticket);
            var (refresh, refreshExp) = await _tokens.IssueRefreshAsync(user.Id, (Guid)user.TenantId, ct);

            SetRefreshCookie(HttpContext, refresh, refreshExp);

            return Ok(new AuthResponseDto(new AccessTokenDto(access, accessExp)));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            var refresh = Request.Cookies["refresh_token"];
            if (string.IsNullOrWhiteSpace(refresh))
                return Unauthorized(new { title = "Unauthorized", detail = "No refresh token." });

            var (ok, current) = await _tokens.ValidateRefreshAsync(refresh, ct);
            if (!ok || current is null)
                return Unauthorized(new { title = "Unauthorized", detail = "Invalid refresh token." });

            var user = await _users.FindByIdAsync(current.UserId.ToString());
            var roles = await _users.GetRolesAsync(user);

            var tenant = await _mediator.Send(new GetTenantByIdQuery((Guid)current.TenantId), ct);
            if (tenant?.Data is null)
                return Unauthorized(new { title = "Unauthorized", detail = "Geçersiz kimlik bilgileri." });

            var ticket = new UserTicket(current.UserId, current.TenantId, "", roles, tenant.Data.Name, user.FullName);
            var (access, accessExp) = _tokens.Create(ticket);

            // Refresh rotate
            var (newRefresh, newExp) = await _tokens.RotateRefreshAsync(current, ct);
            SetRefreshCookie(HttpContext, newRefresh, newExp);

            return Ok(new AuthResponseDto(new AccessTokenDto(access, accessExp)));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var refresh = Request.Cookies["refresh_token"];
            if (!string.IsNullOrWhiteSpace(refresh))
            {
                await _tokens.RevokeRefreshAsync(refresh, ct);
                ClearRefreshCookie(HttpContext);
            }
            return NoContent();
        }
        
    }

    public sealed record LoginRequest(string UserName, string Password,string TenantCode);
    public sealed record AuthResponseDto(AccessTokenDto Data);
    public sealed record AccessTokenDto(string AccessToken, DateTimeOffset ExpiresAt);
}

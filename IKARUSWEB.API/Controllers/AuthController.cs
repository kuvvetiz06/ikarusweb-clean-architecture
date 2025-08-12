using IKARUSWEB.Infrastructure.Auth;
using IKARUSWEB.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IKARUSWEB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signIn;
        private readonly UserManager<AppUser> _users;
        private readonly ITokenService _tokens;

        public AuthController(SignInManager<AppUser> signIn, UserManager<AppUser> users, ITokenService tokens)
        {
            _signIn = signIn; _users = users; _tokens = tokens;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _users.FindByNameAsync(req.UserName);
            if (user is null) return Unauthorized();

            var check = await _signIn.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
            if (!check.Succeeded) return Unauthorized();

            var roles = await _users.GetRolesAsync(user);
            var token = _tokens.Create(user, roles);

            return Ok(new { access_token = token });
        }

        public sealed record LoginRequest(
            [Required] string UserName,
            [Required] string Password
        );
    }
}

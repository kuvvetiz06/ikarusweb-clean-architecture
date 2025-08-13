using IKARUSWEB.Infrastructure.Auth;
using IKARUSWEB.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
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

            var roles = await _users.GetRolesAsync(user); // IList<string> (normalde null olmaz)
            var token = _tokens.Create(user, roles ?? Array.Empty<string>()); // emniyet kemeri

            return Ok(new { access_token = token });
        }

        public sealed record LoginRequest(
            [Required] string UserName,
            [Required] string Password
        );

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var user = new AppUser { UserName = req.UserName, Email = req.Email };
            var result = await _users.CreateAsync(user, req.Password);
            if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));
            return Ok();
        }
        public sealed record RegisterRequest(string UserName, string Email, string Password);
    }
}

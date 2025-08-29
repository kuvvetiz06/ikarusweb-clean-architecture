using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.UI.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        [HttpGet("/account/login"), AllowAnonymous]
        public IActionResult Login(string? returnUrl = null) => View();

        [HttpGet("/account/denied"), AllowAnonymous]
        public IActionResult Denied() => View();
    }
}

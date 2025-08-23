using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.UI.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

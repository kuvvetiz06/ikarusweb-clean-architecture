using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.UI.Controllers
{
    public class HotelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

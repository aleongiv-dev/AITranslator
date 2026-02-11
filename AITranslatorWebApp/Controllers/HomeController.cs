using Microsoft.AspNetCore.Mvc;

namespace AITranslatorWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
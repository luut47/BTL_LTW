using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
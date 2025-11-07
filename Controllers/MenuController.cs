using BTL_LTW.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW.Controllers
{
    public class MenuController : Controller
    {
        public readonly RestaurantDb _db;
        public MenuController(RestaurantDb db) => _db = db;

        public IActionResult Index()
        {
            var categories = _db.MenuCategories.Include(c => c.Items).OrderBy(c => c.Name).ToList();
            return View(categories);
        }
    }
}

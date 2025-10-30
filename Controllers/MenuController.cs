using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Services;

namespace BTL_LTW.Controllers
{
    public class MenuController : Controller
    {
        private readonly IStorage _storage;
        public MenuController(IStorage storage) => _storage = storage;

        public IActionResult Index()
        {
            var categories = _storage.LoadMenuCategories();
            return View(categories);
        }
    }
}

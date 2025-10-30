using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Services;

namespace BTL_LTW.Controllers
{
    public class KitchenController : Controller
    {
        private readonly IStorage _storage;
        public KitchenController(IStorage st) => _storage = st;

        public IActionResult Index()
        {
            var orders = _storage.GetOrders();
            return View(orders);
        }
    }
}

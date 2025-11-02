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
        [HttpPost]
        public IActionResult UpdateItemStatus(string orderId, int menuItemId, string status)
        {
            var ok = _storage.UpdateOrderItemStatus(orderId, menuItemId, status);
            if (!ok) return NotFound();
            return Ok(new { orderId, menuItemId, status });
        }
        public IActionResult ListPartial()
        {
            var orders = _storage.GetOrders()
                                 .Where(o => !o.IsCompleted)
                                 .OrderBy(o => o.CreatedAt)
                                 .ToList();
            return PartialView("_KitchenListPartial", orders);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_LTW.Data;

namespace BTL_LTW.Controllers
{
    public class KitchenController : Controller
    {
        private readonly RestaurantDb _db;
        public KitchenController(RestaurantDb db) => _db = db;

        public IActionResult Index()
        {
            var orders = _db.Orders.Include(o => o.Items).OrderBy(o => o.CreatedAt).ToList();
            return View(orders);
        }
        [HttpPost]
        public IActionResult UpdateItemStatus(string orderId, int menuItemId, string status)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(status))
                return BadRequest();

            var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == orderId);

            if(order == null) return NotFound(); 

            var item = order.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);

            if (item == null) return NotFound();
            item.Status = status;
            if(order.Items.All(i => i.Status == "Ready" || i.Status == "Served"))
            {
                order.Status = "Readt";
                order.IsCompleted = false;
            }

            _db.SaveChanges();
            return Ok(new { orderId, menuItemId, status });
        }
        public IActionResult ListPartial()
        {
            var orders = _db.Orders
                            .Include(o => o.Items)
                            .Where(o => !o.IsCompleted)
                            .OrderBy(o => o.CreatedAt)
                            .ToList();

            return PartialView("_KitchenListPartial", orders);
        }
    }
}

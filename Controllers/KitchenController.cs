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
            return View();
        }
        [HttpGet]
        public IActionResult KitchenPartial()
        {
            var orders = _db.Orders
                            .Include(o => o.Items)
                            .Where(o => !o.IsCompleted)
                            .OrderBy(o => o.CreatedAt)
                            .ToList();
            return PartialView("_KitchenListPartial", orders);
        }
        [HttpPost]
        public IActionResult UpdateStatus([FromBody] UpdateKitchenStatusDto dto)
        {
            var order = _db.Orders.FirstOrDefault(o => o.Id == dto.Id);
            if (order == null) return NotFound();
            order.Status = dto.Status;
            _db.SaveChanges();
            return Json(new { ok = true });
        }

        public class UpdateKitchenStatusDto
        {
            public string Id { get; set; } = "";
            public string Status { get; set; } = "";
        }
        
    }
}

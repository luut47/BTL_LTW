using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Models;
using BTL_LTW.Data;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW.Controllers
{
    public class OrdersController : Controller
    {
        private readonly RestaurantDb _db;

        public OrdersController(RestaurantDb db, IWebHostEnvironment env)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Get(string id)
        {
            var o = _db.Orders.Include(x => x.Items).FirstOrDefault(x => x.Id == id);

            if (o == null) return NotFound();
            return Ok(o);
        }

        [HttpGet]
        public IActionResult List()
        {
            var orders = _db.Orders.Include(o => o.Items).OrderByDescending(o => o.CreatedAt).ToList();
            return Ok(orders);
        }


        [HttpPost]
        public IActionResult Create([FromBody] Order? dto, [FromQuery] string? reservationId)
        {
            if (dto == null)
                return BadRequest("Body rỗng.");

            ModelState.Remove(nameof(Order.Id));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();

                return BadRequest(new { success = false, errors });
            }

            // ===== Tính tiền, gán Id, status, v.v. =====
            var menuDict = _db.MenuItems.ToDictionary(m => m.Id, m => m);
            decimal total = 0m;

            foreach (var it in dto.Items)
            {
                if (!menuDict.TryGetValue(it.MenuItemId, out var mi))
                    return BadRequest($"Menu item không tồn tại (id={it.MenuItemId})");

                it.MenuItemName = mi.Name;
                it.UnitPrice = mi.Price;
                if (it.Qty <= 0) it.Qty = 1;

                total += it.UnitPrice * it.Qty;
            }

            dto.Total = total;
            dto.Id = Guid.NewGuid().ToString();  // ⬅ server tự tạo Id ở đây
            dto.CreatedAt = DateTime.UtcNow;
            dto.Status = "Pending";
            dto.IsCompleted = false;

            // Nếu order này gắn với reservation
            if (!string.IsNullOrWhiteSpace(dto.ReservationId))
                reservationId = dto.ReservationId;

            if (!string.IsNullOrWhiteSpace(reservationId))
            {
                dto.ReservationId = reservationId;
                var r = _db.Reservations.FirstOrDefault(x => x.Id == reservationId);
                if (r != null)
                {
                    r.LinkOrderId = dto.Id;

                    if (!string.IsNullOrWhiteSpace(r.AssignedTable) &&
                        string.IsNullOrWhiteSpace(dto.AssignedTable))
                    {
                        dto.AssignedTable = r.AssignedTable;
                    }
                }
            }

            _db.Orders.Add(dto);
            _db.SaveChanges();

            return Ok(new { success = true, id = dto.Id });
        }



        [HttpPatch]
        public IActionResult UpdateItemStatus(string orderId, int menuItemId, [FromQuery] string status)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return BadRequest();

            var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            var item = order.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
           
            if (item == null)
                return NotFound();

            item.Status = status;

            // Nếu tất cả món đã Served → đóng order
            if (order.Items.All(i => i.Status == "Served"))
            {
                order.Status = "Closed";   // giống FileStorage.UpdateOrderItemStatus
                order.IsCompleted = false;       // có thể set true luôn cho tiện
            }

            _db.SaveChanges();
            return Ok();
        }

        // GET /Orders/Status?id=...
        [HttpGet]
        public IActionResult Status(string id)
        {
            var o = _db.Orders.Include(x => x.Items).FirstOrDefault(x => x.Id == id);

            if (o == null) return NotFound();

            var dto = new
            {
                id = o.Id,
                status = o.Status,
                total = o.Total,
                isCompleted = o.IsCompleted,
                items = o.Items.Select(i => new
                {
                    i.MenuItemId,
                    i.MenuItemName,
                    i.Qty,
                    i.UnitPrice,
                    status = i.Status
                }).ToList()
            };

            return Json(dto);
        }

    }
}

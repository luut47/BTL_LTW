using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Models;
using BTL_LTW.Services;
using System.Text.Json;

namespace BTL_LTW.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IStorage _storage;
        private readonly IWebHostEnvironment _env;
        private readonly JsonSerializerOptions _opt = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

        public OrdersController(IStorage sto, IWebHostEnvironment env)
        {
            _storage = sto;
            _env = env;
        } 

        [HttpGet]
        public IActionResult Get(string id)
        {
            var o = _storage.GetOrder(id);
            if (o == null) return NotFound();
            return Ok(o);
        }

        [HttpGet]
        public IActionResult List()
        {
            var orders = _storage.GetOrders();
            return Ok(orders);
        }


        [HttpPost]
        public IActionResult Create([FromBody] Order? dto, [FromQuery] string? reservationId)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
                return BadRequest("Chưa order gì");

            try
            {
                // ưu tiên lấy ReservationId từ body nếu bạn đã thêm vào model
                if (!string.IsNullOrWhiteSpace(dto?.ReservationId))
                    reservationId = dto.ReservationId;

                var saved = _storage.CreateOrder(dto);

                // nếu có reservationId -> cập nhật lại file reservations.json
                if (!string.IsNullOrWhiteSpace(reservationId))
                {
                    var dataDir = Path.Combine(_env.ContentRootPath, "data");
                    var file = Path.Combine(dataDir, "reservations.json");
                    var list = new List<Reservation>();

                    if (System.IO.File.Exists(file))
                    {
                        var txt = System.IO.File.ReadAllText(file);
                        list = System.Text.Json.JsonSerializer.Deserialize<List<Reservation>>(txt, _opt) ?? new();
                    }

                    var r = list.FirstOrDefault(x => x.Id == reservationId);
                    if (r != null)
                    {
                        r.LinkOrderId = saved.Id;      // thuộc tính bạn dùng để hiển thị cột Hóa đơn
                                                         // nếu Reservation có AssignedTable thì có thể set luôn saved.AssignedTable = r.AssignedTable;
                    }

                    System.IO.File.WriteAllText(file,
                        System.Text.Json.JsonSerializer.Serialize(list, _opt));
                }

                return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPatch]
        public IActionResult UpdateItemStatus(string orderId, int menuItemId, [FromQuery] string status)
        {
            var ok = _storage.UpdateOrderItemStatus(orderId, menuItemId, status);
            if (!ok) return NotFound();
            return Ok();
        }
        [HttpGet]
        public IActionResult Status(string id)
        {
            var o = _storage.GetOrder(id);
            if (o == null) return NotFound();

            var dto = new
            {
                id = o.Id,
                status = o.Status,
                total = o.Total,
                isCompleted = o.IsCompleted,
                items = o.Items.Select(i => new {
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

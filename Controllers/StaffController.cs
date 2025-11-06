using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Services;
using BTL_LTW.Models;
using System.Text.Json;

namespace BTL_LTW.Controllers
{
    public class StaffController : Controller
    {
        private readonly IStorage _storage;
        public StaffController(IStorage storage) => _storage = storage;

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if(username == "staff" && password == "abc123")
            {
                HttpContext.Session.SetString("isStaff", "1");
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Error = "Sai thông tin đăng nhập";
            return View();
        }
        private bool IsAuthenticated() => HttpContext.Session.GetString("isStaff") == "1";
        private IActionResult Protect()
        {
            if (!IsAuthenticated()) return RedirectToAction(nameof(Login));
            return null!;
        }
        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("isStaff") != "1")
                return RedirectToAction(nameof(Login));

            var allOrders = _storage.GetOrders();

            // CHỈ hiển thị đơn tại quán + đơn mang về
            // => các order không gắn với Reservation
            var orders = allOrders
                .Where(o => string.IsNullOrEmpty(o.ReservationId))
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            var tables = _storage.GetTables();
            ViewBag.Tables = tables;

            return View(orders);
        }

        [HttpPost]
        public IActionResult AssignTable(string orderId, string tableId)
        {
            if (!IsAuthenticated()) return Unauthorized();
            var ok = _storage.AssignTableOrder(tableId, orderId);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult PrintBill(string id)
        {
            if (!IsAuthenticated()) return RedirectToAction(nameof(Login));
            var o = _storage.GetOrder(id);
            if (o == null) return NotFound();
            return View(o);
        }
        [HttpPost]
        public IActionResult CompletedOrder(string id, string? returnTo)
        {
            var orders = _storage.GetOrders();
            var o = orders.FirstOrDefault(x => x.Id == id);
            if (o == null) return NotFound();

            o.IsCompleted = true;
            o.Status = "Completed";

            _storage.SaveOrders(orders);

            if (!string.IsNullOrEmpty(returnTo) && returnTo == "reservations")
                return RedirectToAction("Reservations");

            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("isStaff");
            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public IActionResult Reservations()
        {
            if (HttpContext.Session.GetString("isStaff") != "1")
                return RedirectToAction(nameof(Login));

            // đọc reservation như hiện tại của bạn:
            var reservations = _storage.ReadReservations()  // hoặc _storage.ReadReservations()
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var tables = _storage.GetTables();      // hoặc _storage.GetTables() tuỳ interface bạn
            ViewBag.Tables = tables;

            var allOrders = _storage.GetOrders();
            // map: reservationId -> order từ LinkOrderId
            var ordersByReservation = new Dictionary<string, Order>();

            foreach (var r in reservations)
            {
                if (!string.IsNullOrEmpty(r.LinkOrderId))
                {
                    var o = allOrders.FirstOrDefault(x => x.Id == r.LinkOrderId);
                    if (o != null)
                        ordersByReservation[r.Id] = o;
                }
            }

            ViewBag.OrdersByReservation = ordersByReservation;

            return View(reservations);
        }

        [HttpGet]
        public IActionResult ReservationsPartial()
        {
            if (HttpContext.Session.GetString("isStaff") != "1")
                return Unauthorized();

            var reservations = _storage.ReadReservations()
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var tables = _storage.GetTables();      // hoặc GetTables()
            ViewBag.Tables = tables;

            var allOrders = _storage.GetOrders();
            var ordersByReservation = new Dictionary<string, Order>();
            foreach (var r in reservations)
            {
                if (!string.IsNullOrEmpty(r.LinkOrderId))
                {
                    var o = allOrders.FirstOrDefault(x => x.Id == r.LinkOrderId);
                    if (o != null)
                        ordersByReservation[r.Id] = o;
                }
            }

            ViewBag.OrdersByReservation = ordersByReservation;

            return PartialView("_ReservationsPartial", reservations);
        }

        [HttpPost]
        public IActionResult AssignTableReservation(string reservationId, string tableId)
        {
            if (string.IsNullOrWhiteSpace(reservationId) || string.IsNullOrWhiteSpace(tableId))
                return RedirectToAction(nameof(Reservations));

            _storage.AssignTableReservation(reservationId, tableId);
            return RedirectToAction(nameof(Reservations));
        }

        [HttpPost]
        public IActionResult ReleaseTableReservation(string reservationId)
        {
            if (string.IsNullOrWhiteSpace(reservationId))
                return RedirectToAction(nameof(Reservations));

            _storage.ReleaseTableByReservation(reservationId);
            return RedirectToAction(nameof(Reservations));
        }
        public class ReservationOrderItemDto
        {
            public int MenuItemId { get; set; }
            public int Qty { get; set; }
            public string? Note { get; set; }
        }
        public class CreateOrderFromReservationDto
        {
            public string ReservationId { get; set; } = "";
            public List<ReservationOrderItemDto> Items { get; set; } = new();
        }

        [HttpPost]
        public IActionResult CreateOrderFromReservation([FromBody] CreateOrderFromReservationDto dto)
        {
            if (HttpContext.Session.GetString("isStaff") != "1") return Unauthorized();
            if (dto == null || string.IsNullOrWhiteSpace(dto.ReservationId) || dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Thiếu dữ liệu");

            try
            {
                var items = dto.Items.Select(x => (x.MenuItemId, Math.Max(1, x.Qty), x.Note)).ToList();
                var order = _storage.CreateOrderFromReservation(dto.ReservationId, items);
                return Json(new { ok = true, orderId = order.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}

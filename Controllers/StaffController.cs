using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_LTW.Data;
using BTL_LTW.Models;

namespace BTL_LTW.Controllers
{
    public class StaffController : Controller
    {
        private readonly RestaurantDb _db;

        public StaffController(RestaurantDb db)
        {
            _db = db;
        }

        // ================== LOGIN / LOGOUT ==================

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "staff" && password == "staff123")
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

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("isStaff");
            return RedirectToAction(nameof(Login));
        }

        // ================== ĐƠN TẠI QUÁN / ĐƠN MANG VỀ ==================

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction(nameof(Login));

            // CHỈ hiển thị đơn tại quán + đơn mang về
            // => các order KHÔNG gắn với Reservation
            var orders = _db.Orders
                            .Where(o => string.IsNullOrEmpty(o.ReservationId))
                            .OrderByDescending(o => o.CreatedAt)
                            .ToList();

            var tables = _db.TableInfos.ToList();
            ViewBag.Tables = tables;

            return View(orders);
        }

        [HttpPost]
        public IActionResult AssignTable(string orderId, string tableId)
        {
            if (!IsAuthenticated()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(tableId))
                return RedirectToAction(nameof(Index));

            var order = _db.Orders.FirstOrDefault(o => o.Id == orderId);
            var table = _db.TableInfos.FirstOrDefault(t => t.Id == tableId);

            if (order == null || table == null)
                return RedirectToAction(nameof(Index));

            // Gán bàn cho order
            order.AssignedTable = table.Id;

            // Đánh dấu bàn đang có người
            table.IsOccuped = true;
            table.OccupiedById = order.Id;

            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult PrintBill(string id)
        {
            if (!IsAuthenticated())
                return RedirectToAction(nameof(Login));

            var o = _db.Orders
                       .FirstOrDefault(x => x.Id == id);

            if (o == null) return NotFound();

            return View(o);
        }

        [HttpPost]
        public IActionResult CompletedOrder(string id, string? returnTo)
        {
            if (!IsAuthenticated())
                return RedirectToAction(nameof(Login));

            var o = _db.Orders.FirstOrDefault(x => x.Id == id);
            if (o == null) return NotFound();

            o.IsCompleted = true;
            o.Status = "Completed";

            // Giải phóng bàn nếu có
            if (!string.IsNullOrEmpty(o.AssignedTable))
            {
                var table = _db.TableInfos.FirstOrDefault(t => t.Id == o.AssignedTable);
                if (table != null)
                {
                    table.IsOccuped = false;
                    table.OccupiedById = null;
                }
            }

            _db.SaveChanges();

            if (!string.IsNullOrEmpty(returnTo) && returnTo == "reservations")
                return RedirectToAction(nameof(Reservations));

            return RedirectToAction(nameof(Index));
        }

        // ================== RESERVATIONS (ĐẶT BÀN) ==================

        [HttpGet]
        public IActionResult Reservations()
        {
            if (!IsAuthenticated())
                return RedirectToAction(nameof(Login));

            var reservations = _db.Reservations
                                  .OrderByDescending(r => r.CreatedAt)
                                  .ToList();

            var tables = _db.TableInfos.ToList();
            ViewBag.Tables = tables;

            var allOrders = _db.Orders.ToList();

            // map: reservationId -> order theo LinkOrderId
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
            if (!IsAuthenticated())
                return Unauthorized();

            var reservations = _db.Reservations
                                  .OrderByDescending(r => r.CreatedAt)
                                  .ToList();

            var tables = _db.TableInfos.ToList();
            ViewBag.Tables = tables;

            var allOrders = _db.Orders.ToList();
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
            if (!IsAuthenticated())
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(reservationId) || string.IsNullOrWhiteSpace(tableId))
                return RedirectToAction(nameof(Reservations));

            var reservation = _db.Reservations.FirstOrDefault(r => r.Id == reservationId);
            var table = _db.TableInfos.FirstOrDefault(t => t.Id == tableId);

            if (reservation == null || table == null)
                return RedirectToAction(nameof(Reservations));

            // Gán bàn cho reservation
            reservation.AssignedTable = table.Id;

            table.IsOccuped = true;
            table.OccupiedById = reservation.Id;

            _db.SaveChanges();

            return RedirectToAction(nameof(Reservations));
        }

        [HttpPost]
        public IActionResult ReleaseTableReservation(string reservationId)
        {
            if (!IsAuthenticated())
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(reservationId))
                return RedirectToAction(nameof(Reservations));

            var reservation = _db.Reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation == null)
                return RedirectToAction(nameof(Reservations));

            if (!string.IsNullOrEmpty(reservation.AssignedTable))
            {
                var table = _db.TableInfos.FirstOrDefault(t => t.Id == reservation.AssignedTable);
                if (table != null)
                {
                    table.IsOccuped = false;
                    table.OccupiedById = null;
                }
            }

            reservation.AssignedTable = null;

            _db.SaveChanges();

            return RedirectToAction(nameof(Reservations));
        }

        // ================== DTOs TẠO ORDER TỪ RESERVATION ==================

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
            if (!IsAuthenticated()) return Unauthorized();

            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.ReservationId) ||
                dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Thiếu dữ liệu");

            try
            {
                var reservation = _db.Reservations
                                     .FirstOrDefault(r => r.Id == dto.ReservationId);

                if (reservation == null)
                    return BadRequest("Không tìm thấy reservation");

                var menuItemIds = dto.Items.Select(x => x.MenuItemId).ToList();

                var menuItems = _db.MenuItems
                                   .Where(m => menuItemIds.Contains(m.Id))
                                   .ToList();

                if (menuItems.Count != menuItemIds.Count)
                    return BadRequest("Một số món không tồn tại trong menu");

                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    ReservationId = reservation.Id,
                    AssignedTable = reservation.AssignedTable, // nếu reservation đã được gán bàn
                    Status = "Pending",
                    IsCompleted = false,
                    Items = new List<OrderItem>()
                };

                foreach (var x in dto.Items)
                {
                    var mi = menuItems.First(m => m.Id == x.MenuItemId);
                    var qty = Math.Max(1, x.Qty);

                    var orderItem = new OrderItem
                    {
                        MenuItemId = mi.Id,
                        MenuItemName = mi.Name,
                        Qty = qty,
                        UnitPrice = mi.Price,
                        Note = x.Note,
                        Status = "Pending"
                    };

                    order.Items.Add(orderItem);
                }

                // Tính tổng tiền
                order.Total = order.Items.Sum(i => i.Qty * i.UnitPrice);

                // Link order lại với reservation
                reservation.LinkOrderId = order.Id;

                _db.Orders.Add(order);
                _db.SaveChanges();

                return Json(new { ok = true, orderId = order.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

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

            var orders = _storage.GetOrders().OrderByDescending(o => o.CreatedAt).ToList();
            var tables = _storage.GetTables();                   
            ViewBag.Tables = tables;                              
            ViewBag.OccupiedCount = tables.Count(t => t.IsOccuped);
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
        public IActionResult CompletedOrder(string id)
        {
            if (!IsAuthenticated()) return Unauthorized();
            _storage.MarkOrder(id);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("isStaff");
            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        [HttpGet]
        public IActionResult Reservations()
        {
            if (HttpContext.Session.GetString("isStaff") != "1")
                return RedirectToAction(nameof(Login));

            ViewBag.Tables = _storage.GetTables();        // để dropdown có dữ liệu
            var list = _storage.ReadReservations()
                               .OrderByDescending(x => x.CreatedAt)
                               .ToList();
            return View(list); // dùng view mạnh kiểu: Views/Staff/Reservations.cshtml (model: List<Reservation>)
        }

        [HttpGet]
        public IActionResult ReservationsPartial()
        {
            if (HttpContext.Session.GetString("isStaff") != "1")
                return Unauthorized();

            ViewBag.Tables = _storage.GetTables();        // partial cũng cần
            var list = _storage.ReadReservations()
                               .OrderByDescending(x => x.CreatedAt)
                               .ToList();
            return PartialView("_ReservationsPartial", list);
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


    }
}

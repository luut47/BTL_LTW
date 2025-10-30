using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Services;
using BTL_LTW.Models;

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
            if (!IsAuthenticated()) return RedirectToAction(nameof(Login));
            var orders = _storage.GetOrders().OrderByDescending(o => o.CreatedAt).ToList();
            var tables = _storage.GetTables();
            ViewBag.Tables = tables;
            ViewBag.OccupiedCount = _storage.GetOccupiedCount();
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
    }
}

using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Models;
using System.Text.Json;

namespace BTL_LTW.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly JsonSerializerOptions _opt = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

        public ReservationController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public IActionResult Create([FromForm] Reservation model)
        {
            if (model == null) return BadRequest("Dữ liệu rỗng");
            if (string.IsNullOrWhiteSpace(model.CustomerName) || string.IsNullOrWhiteSpace(model.Phone))
                return BadRequest("Vui lòng nhập tên và số điện thoại.");

            var dataDir = Path.Combine(_env.ContentRootPath, "data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            var file = Path.Combine(dataDir, "reservations.json");
            List<Reservation> list;
            if (System.IO.File.Exists(file))
            {
                try
                {
                    var txt = System.IO.File.ReadAllText(file);
                    list = JsonSerializer.Deserialize<List<Reservation>>(txt, _opt) ?? new List<Reservation>();
                }
                catch
                {
                    list = new List<Reservation>();
                }
            }
            else
            {
                list = new List<Reservation>();
            }

            model.Id = Guid.NewGuid().ToString();
            model.CreatedAt = DateTime.UtcNow;
            list.Add(model);

            System.IO.File.WriteAllText(file, JsonSerializer.Serialize(list, _opt));
            return Ok(new { success = true, id = model.Id });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Models;
using BTL_LTW.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace BTL_LTW.Controllers
{
    public class ReservationController : Controller
    {
        private readonly RestaurantDb _db;

        public ReservationController(RestaurantDb db)
        {
            _db = db;
        }

        [HttpPost]
        public IActionResult Create([FromForm] Reservation model)
        {
            if (model == null)
                return BadRequest("Dữ liệu rỗng");

            //if (string.IsNullOrWhiteSpace(model.CustomerName) || string.IsNullOrWhiteSpace(model.Phone))
            //    return BadRequest("Vui lòng nhập tên và số điện thoại.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();

                return BadRequest(ModelState);
            }

            model.Id = Guid.NewGuid().ToString();
            model.CreatedAt = DateTime.UtcNow; 

            _db.Reservations.Add(model);
            _db.SaveChanges();  

            return Ok(new { success = true, id = model.Id });
        }

        [HttpGet]
        public IActionResult List()
        {
            var reservations = _db.Reservations
                                  .OrderByDescending(r => r.CreatedAt)
                                  .ToList();  

            return Ok(reservations);
        }

        [HttpGet]
        public IActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id không hợp lệ.");

            var reservation = _db.Reservations
                                 .FirstOrDefault(r => r.Id == id);  

            if (reservation == null)
                return NotFound();

            return Ok(reservation);
        }
    }
}

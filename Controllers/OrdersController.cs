using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Models;
using BTL_LTW.Services;

namespace BTL_LTW.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IStorage _storage;
        public OrdersController(IStorage sto) => _storage = sto;

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

        //[HttpPost]
        //public IActionResult Create([FromBody] Order? dto)
        //{
        //    if (dto == null || dto.Items == null || !dto.Items.Any()) return BadRequest("Chưa order gì");

        //    try
        //    {
        //        // CreateOrder sẽ validate ids & gán giá
        //        var saved = _storage.CreateOrder(dto);
        //        return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        [HttpPost]
        public IActionResult Create([FromBody] Order? dto)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any()) return BadRequest("Chưa order gì");

            try
            {
                var saved = _storage.CreateOrder(dto);
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
    }
}

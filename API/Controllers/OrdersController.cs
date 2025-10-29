using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu đăng nhập để tạo đơn hàng
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // POST: /api/orders
        // Tạo đơn hàng từ các sản phẩm đã chọn trong giỏ
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto orderRequest)
        {
            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(GetUserId(), orderRequest);
                if (order == null)
                {
                    return BadRequest(new { message = "Giỏ hàng rỗng hoặc không thể tạo đơn hàng." });
                }
                // Trả về 201 Created cùng với thông tin đơn hàng chi tiết
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: /api/orders/{id} (Ví dụ API lấy chi tiết đơn hàng sau này)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            // Logic lấy đơn hàng theo Id và kiểm tra quyền sở hữu...
            // var order = await _orderService.GetOrderByIdAsync(GetUserId(), id);
            // if (order == null) return NotFound();
            // return Ok(order);
            return Ok($"Lấy thông tin đơn hàng {id}"); // Placeholder
        }
    }
}

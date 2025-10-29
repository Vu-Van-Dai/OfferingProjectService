using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Application.Dtos.CartDtos;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // BẮT BUỘC đăng nhập
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Lấy UserID từ Token
        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // GET: /api/cart
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            try { return Ok(await _cartService.GetCartAsync(GetUserId())); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // POST: /api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddItemToCartDto itemDto)
        {
            try { return Ok(await _cartService.AddItemToCartAsync(GetUserId(), itemDto)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT: /api/cart/items/{cartItemId}/quantity
        [HttpPut("items/{cartItemId}/quantity")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            try { return Ok(await _cartService.UpdateItemQuantityAsync(GetUserId(), cartItemId, dto.Quantity)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT: /api/cart/items/{cartItemId}/select
        [HttpPut("items/{cartItemId}/select")]
        public async Task<IActionResult> SelectItem(int cartItemId, [FromBody] SelectCartItemDto dto)
        {
            try { return Ok(await _cartService.SetItemSelectionAsync(GetUserId(), cartItemId, dto.IsSelected)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // DELETE: /api/cart/items/{cartItemId}
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            try { return Ok(await _cartService.RemoveItemAsync(GetUserId(), cartItemId)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}

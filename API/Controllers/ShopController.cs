using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/shop")]
    [Authorize(Roles = "Shop")]
    public class ShopController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IProductService _productService;
        private readonly IShopProfileService _shopProfileService;
        private readonly IOrderItemService _orderItemService;
        private readonly IReviewQueryService _reviewQueryService;
        private readonly IStatisticsService _statisticsService;

        public ShopController(
            IDashboardService dashboardService, IProductService productService,
            IShopProfileService shopProfileService, IOrderItemService orderItemService,
            IReviewQueryService reviewQueryService, IStatisticsService statisticsService)
        {
            _dashboardService = dashboardService; _productService = productService;
            _shopProfileService = shopProfileService; _orderItemService = orderItemService;
            _reviewQueryService = reviewQueryService; _statisticsService = statisticsService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private int GetShopId() => int.Parse(User.FindFirstValue("ShopId") ?? throw new UnauthorizedAccessException("ShopId claim is missing."));

        // --- Dashboard ---
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard() => Ok(await _dashboardService.GetShopDashboardAsync(GetShopId()));

        // --- Shop Profile (Settings) ---
        [HttpGet("profile")]
        public async Task<IActionResult> GetShopProfile()
        {
            var profile = await _shopProfileService.GetShopProfileByUserIdAsync(GetUserId());
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateShopProfile([FromForm] UpdateShopProfileDto dto)
        {
            var result = await _shopProfileService.UpdateShopProfileAsync(GetUserId(), dto);
            if (!result) return NotFound();
            return NoContent();
        }

        // --- Product Management ---
        [HttpGet("products")]
        public async Task<IActionResult> GetMyShopProducts() => Ok(await _productService.GetByShopIdAsync(GetShopId()));

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetMyShopProductById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null || product.ShopId != GetShopId()) return NotFound();
            return Ok(product); // Cần ProductResponseDto nếu muốn map
        }

        [HttpPost("products")]
        public async Task<IActionResult> CreateMyProduct([FromForm] CreateProductDto dto)
        {
            try { var p = await _productService.CreateAsync(dto, GetUserId()); return CreatedAtAction(nameof(GetMyShopProductById), new { id = p.Id }, p); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateMyProduct(int id, [FromForm] UpdateProductDto dto)
        {
            try { var r = await _productService.UpdateAsync(id, dto, GetUserId()); if (!r) return NotFound(); return NoContent(); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteMyProduct(int id)
        {
            try { var r = await _productService.DeleteAsync(id, GetUserId()); if (!r) return NotFound(); return NoContent(); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // --- Order Management ---
        [HttpGet("orders")]
        public async Task<IActionResult> GetMyOrderItems([FromQuery] OrderItemShopStatus? status) => Ok(await _orderItemService.GetOrderItemsForShopAsync(GetShopId(), status));

        [HttpPut("orders/items/{orderItemId}/status")]
        public async Task<IActionResult> UpdateOrderItemStatus(int orderItemId, [FromBody] UpdateOrderItemStatusDto dto)
        {
            var r = await _orderItemService.UpdateOrderItemStatusAsync(GetShopId(), orderItemId, dto.NewStatus);
            if (!r) return NotFound();
            return NoContent();
        }

        // --- Review Management ---
        [HttpGet("reviews")]
        public async Task<IActionResult> GetMyReviews() => Ok(await _reviewQueryService.GetReviewsForShopAsync(GetShopId()));

        // --- Statistics ---
        [HttpGet("statistics")]
        public async Task<IActionResult> GetMyStatistics() => Ok(await _statisticsService.GetShopStatisticsAsync(GetShopId()));
    }
}

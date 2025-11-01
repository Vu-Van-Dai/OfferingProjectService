using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminCategoryService _categoryService;
        private readonly IActivityLogService _logService;
        private readonly IAdminShopService _shopService;
        private readonly IAdminConfigService _configService; // Đã thêm
        private readonly IAdminProductService _productService; // Đã thêm
        private readonly IAdminDashboardService _dashboardService;

        public AdminController(
            IAdminCategoryService categoryService,
            IActivityLogService logService,
            IAdminShopService shopService,
            IAdminConfigService configService, // Đã thêm
            IAdminProductService productService,
            IAdminDashboardService dashboardService) // Đã thêm
        {
            _categoryService = categoryService;
            _logService = logService;
            _shopService = shopService;
            _configService = configService; // Đã thêm
            _productService = productService; // Đã thêm
            _dashboardService = dashboardService;
        }

        // === 1. Quản lý Danh Mục ===

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
            => Ok(await _categoryService.GetAllCategoriesAsync());

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateCategoryAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("categories/{id}/visibility")]
        public async Task<IActionResult> ToggleCategoryVisibility(int id, [FromBody] ToggleVisibilityDto dto)
        {
            var result = await _categoryService.ToggleCategoryVisibilityAsync(id, dto.IsHidden);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("categories/reorder")]
        public async Task<IActionResult> ReorderCategories([FromBody] ReorderCategoriesDto dto)
        {
            var result = await _categoryService.ReorderCategoriesAsync(dto.OrderedCategoryIds);
            if (!result) return BadRequest("Lỗi sắp xếp.");
            return NoContent();
        }

        // === 2. Quản lý Shop ===

        [HttpGet("shops")]
        public async Task<IActionResult> GetShops()
            => Ok(await _shopService.GetAllShopsAsync());

        [HttpPost("shops/create-new")]
        public async Task<IActionResult> CreateShopAccount([FromBody] AdminCreateShopDto dto)
        {
            try { return Ok(await _shopService.CreateShopAccountAsync(dto)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("shops/convert-guest")]
        public async Task<IActionResult> ConvertGuestToShop([FromBody] AdminConvertGuestDto dto)
        {
            try { return Ok(await _shopService.ConvertGuestToShopAsync(dto)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("shops/{id}")]
        public async Task<IActionResult> UpdateShopInfo(int id, [FromBody] AdminUpdateShopDto dto)
        {
            var result = await _shopService.UpdateShopInfoAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("shops/{id}/status")]
        public async Task<IActionResult> ToggleShopLock(int id, [FromBody] ToggleShopLockDto dto)
        {
            var result = await _shopService.ToggleShopLockAsync(id, dto.IsLocked);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("shops/{id}/reset-password")]
        public async Task<IActionResult> ResetShopPassword(int id, [FromBody] AdminResetPasswordDto dto)
        {
            var result = await _shopService.ResetShopPasswordAsync(id, dto.NewPassword);
            if (!result) return NotFound();
            return Ok(new { message = "Reset mật khẩu thành công." });
        }

        // === 3. Quản lý Sản phẩm (Giám sát) ===

        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            return Ok(await _productService.GetAllProductsAsync());
        }

        [HttpPut("products/{id}/visibility")]
        public async Task<IActionResult> ToggleProductVisibility(int id, [FromBody] ToggleVisibilityDto dto)
        {
            var result = await _productService.ToggleProductVisibilityAsync(id, dto.IsHidden);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("products/{id}/change-category")]
        public async Task<IActionResult> ChangeProductCategory(int id, [FromBody] AdminChangeProductCategoryDto dto)
        {
            var result = await _productService.ChangeProductCategoryAsync(id, dto.NewCategoryId);
            if (!result) return NotFound(new { message = "Không tìm thấy sản phẩm hoặc danh mục mới." });
            return NoContent();
        }

        // === 4. Quản lý Cấu Hình (Hoa hồng) ===

        [HttpGet("config/commissions")]
        public async Task<IActionResult> GetCommissionSettings()
            => Ok(await _configService.GetCommissionSettingsAsync());

        [HttpPut("config/commissions/default")]
        public async Task<IActionResult> SetDefaultCommission([FromBody] UpdateCommissionRateDto dto)
        {
            return Ok(await _configService.SetDefaultCommissionAsync(dto.Rate));
        }

        [HttpPut("config/commissions/shop/{shopId}")]
        public async Task<IActionResult> SetShopCommission(int shopId, [FromBody] UpdateCommissionRateDto dto)
        {
            var result = await _configService.SetShopCommissionAsync(shopId, dto.Rate);
            if (!result) return NotFound(new { message = "Không tìm thấy Shop." });
            return Ok(new { message = "Cập nhật hoa hồng riêng thành công." });
        }

        // === 5. Xem Doanh thu ===

        [HttpGet("dashboard/revenue-stats")]
        public async Task<IActionResult> GetRevenueStats()
        {
            return Ok(await _dashboardService.GetRevenueStatsAsync());
        }

        [HttpGet("dashboard/revenue-by-shop")]
        public async Task<IActionResult> GetRevenueByShop()
        {
            return Ok(await _dashboardService.GetRevenueByShopAsync());
        }

        // === 6. Nhật ký Hoạt động ===

        [HttpGet("logs")]
        public async Task<IActionResult> GetActivityLogs([FromQuery] int count = 20)
        {
            return Ok(await _logService.GetLatestLogsAsync(count));
        }

    }
}

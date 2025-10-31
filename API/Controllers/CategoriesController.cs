using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.Dtos;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        // (Giả sử bạn đã tạo ICategoryService và IProductService)
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public CategoriesController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        // API cho trang "Danh Mục Sản Phẩm" (Hình 4)
        // GET: /api/categories
        [HttpGet]
        [HttpGet] // Trả về List<CategorySummaryDto>
        public async Task<ActionResult<IEnumerable<CategorySummaryDto>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        // API cho các trang danh mục con (Hình 2, 3, 5)
        // GET: /api/categories/1/products (lấy sản phẩm của "Hoa Tươi")
        [HttpGet("{categoryId}/products")] // Trả về List<ProductResponseDto>
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetByCategoryIdAsync(categoryId);
            return Ok(products);
        }

        // === API ADMIN ===
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto categoryDto)
        {
            var newCategory = await _categoryService.CreateAsync(categoryDto);
            return CreatedAtAction(nameof(GetAllCategories), new { id = newCategory.Id }, newCategory);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto categoryDto)
        {
            var result = await _categoryService.UpdateAsync(id, categoryDto);
            if (!result) return NotFound();
            return NoContent(); // 204 No Content - Cập nhật thành công
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent(); // 204 No Content - Xóa thành công
        }
    }
}

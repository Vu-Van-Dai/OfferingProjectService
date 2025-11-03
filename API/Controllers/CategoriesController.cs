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
        public async Task<ActionResult<IEnumerable<CategorySummaryDto>>> GetAllCategoriesSummary()
        {
            var categories = await _categoryService.GetAllSummariesAsync();
            return Ok(categories);
        }

        // API MỚI: Cho trang "Chi tiết Danh mục" (lấy tiêu đề/mô tả)
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDetailDto>> GetCategoryDetails(int id)
        {
            var categoryDetails = await _categoryService.GetCategoryDetailsByIdAsync(id);
            if (categoryDetails == null) return NotFound();
            return Ok(categoryDetails);
        }

        // API cho trang "Chi tiết Danh mục" (lấy sản phẩm)
        [HttpGet("{categoryId}/products")]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetByCategoryIdAsync(categoryId);
            return Ok(products);
        }
    }
}

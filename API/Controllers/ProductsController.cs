using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        // Bỏ IReviewService nếu bạn chỉ gọi API /api/products/{id}/reviews
        // private readonly IReviewService _reviewService; 

        public ProductsController(IProductService productService /*, IReviewService reviewService*/)
        {
            _productService = productService;
            // _reviewService = reviewService;
        }

        // === API CÔNG KHAI ===

        // GET: /api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        // GET: /api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetProductDetails(int id)
        {
            var productDto = await _productService.GetByIdAsync(id);
            if (productDto == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            // Lấy sản phẩm liên quan (đã là DTO)
            var relatedProducts = (await _productService.GetByCategoryIdAsync(productDto.ProductCategoryId))
                                    .Where(p => p.Id != id)
                                    .Take(4);

            return Ok(new
            {
                Product = productDto,
                RelatedProducts = relatedProducts
                // FE sẽ gọi API /api/products/{id}/reviews để lấy reviews
            });
        }

        // === API QUẢN LÝ CỦA SHOP ===

        [HttpPost]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto productDto) // SỬA: [FromForm]
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                // 1. Tạo sản phẩm (Hàm này trả về Entity Product)
                var newProductEntity = await _productService.CreateAsync(productDto, userId);

                // 2. LẤY LẠI SẢN PHẨM DƯỚI DẠNG DTO "AN TOÀN"
                var productDtoResponse = await _productService.GetByIdAsync(newProductEntity.Id);

                // 3. Trả về DTO
                return CreatedAtAction(nameof(GetProductDetails), new { id = newProductEntity.Id }, productDtoResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto productDto) // SỬA: [FromForm]
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var result = await _productService.UpdateAsync(id, productDto, userId);
                if (!result) return NotFound(new { message = "Không tìm thấy sản phẩm." });
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var result = await _productService.DeleteAsync(id, userId);
                if (!result) return NotFound(new { message = "Không tìm thấy sản phẩm." });
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
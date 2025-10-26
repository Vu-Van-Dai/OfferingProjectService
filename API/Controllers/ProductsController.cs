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

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // API cho yêu cầu "hiện tất cả các sản phẩm" (Hình 1)
        // GET: /api/products
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        // API để xem chi tiết 1 sản phẩm
        // GET: /api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _productService.GetByIdAsync(id); // Repo đã Include Shop, Reviews
            if (product == null) return NotFound();

            // Lấy sản phẩm liên quan (cùng danh mục, trừ sản phẩm này)
            var relatedProducts = (await _productService.GetByCategoryIdAsync(product.ProductCategoryId))
                                    .Where(p => p.Id != id)
                                    .Take(4);

            return Ok(new
            {
                Product = product,
                RelatedProducts = relatedProducts
                // Reviews đã nằm trong product.Reviews
            });
        }

        // === API ADMIN ===
        [HttpPost]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var newProduct = await _productService.CreateAsync(productDto, userId);
                return CreatedAtAction(nameof(GetProductDetails), new { id = newProduct.Id }, newProduct);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var result = await _productService.UpdateAsync(id, productDto, userId);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
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
                if (!result) return NotFound();
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
        }
    }
}
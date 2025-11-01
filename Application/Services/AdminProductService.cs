using Application.Dtos;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AdminProductService : IAdminProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo; // Cần để kiểm tra Category mới
        private readonly IActivityLogService _logService;

        public AdminProductService(IProductRepository productRepo, ICategoryRepository categoryRepo, IActivityLogService logService)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _logService = logService;
        }

        public async Task<IEnumerable<AdminProductListResponseDto>> GetAllProductsAsync()
        {
            var products = await _productRepo.GetAllWithShopAndCategoryAsync();

            // Map sang DTO
            return products.Select(p => new AdminProductListResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                ShopName = p.Shop?.Name ?? "N/A",
                CategoryName = p.ProductCategory?.Name ?? "N/A",
                BasePrice = p.BasePrice,
                IsHidden = p.IsHidden
            });
        }

        public async Task<bool> ToggleProductVisibilityAsync(int productId, bool isHidden)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) return false;

            product.IsHidden = isHidden;
            _productRepo.Update(product);
            await _productRepo.SaveChangesAsync();

            var action = isHidden ? "Ẩn" : "Hiện";
            await _logService.LogAsync("Admin", "Ẩn/Hiện Sản Phẩm", $"{action} sản phẩm: {product.Name} (ID: {productId})");
            return true;
        }

        public async Task<bool> ChangeProductCategoryAsync(int productId, int newCategoryId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            var newCategory = await _categoryRepo.GetByIdAsync(newCategoryId);

            // Đảm bảo cả 2 đều tồn tại
            if (product == null || newCategory == null) return false;

            product.ProductCategoryId = newCategoryId;
            _productRepo.Update(product);
            await _productRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Chuyển Danh Mục SP", $"Đã chuyển SP {product.Name} sang danh mục {newCategory.Name}");
            return true;
        }
    }
}

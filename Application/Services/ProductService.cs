using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public ProductService(IProductRepository productRepository, IUserRepository userRepository)
        {
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _productRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product> CreateAsync(CreateProductDto productDto)
        {
            // Chuyển đổi (map) DTO thành Entity
            var newProduct = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Features = productDto.Features,
                ImageUrl = productDto.ImageUrl,
                IsPopular = productDto.IsPopular,
                BasePrice = productDto.BasePrice,
                MaxPrice = productDto.MaxPrice,
                ProductCategoryId = productDto.ProductCategoryId
            };

            await _productRepository.AddAsync(newProduct);
            await _productRepository.SaveChangesAsync();

            return newProduct;
        }
        public async Task<bool> UpdateAsync(int id, UpdateProductDto productDto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null) return false;

            // Cập nhật thuộc tính
            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Features = productDto.Features;
            existingProduct.ImageUrl = productDto.ImageUrl;
            existingProduct.IsPopular = productDto.IsPopular;
            existingProduct.BasePrice = productDto.BasePrice;
            existingProduct.MaxPrice = productDto.MaxPrice;
            existingProduct.ProductCategoryId = productDto.ProductCategoryId;

            _productRepository.Update(existingProduct);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetByShopIdAsync(int shopId)
        {
            return await _productRepository.GetByShopIdAsync(shopId);
        }

        public async Task<Product> CreateAsync(CreateProductDto productDto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.ShopId == null)
                throw new UnauthorizedAccessException("Người dùng này không sở hữu cửa hàng nào.");

            var newProduct = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Features = productDto.Features,
                ImageUrl = productDto.ImageUrl,
                IsPopular = productDto.IsPopular,
                BasePrice = productDto.BasePrice,
                MaxPrice = productDto.MaxPrice,
                StockQuantity = productDto.StockQuantity,
                Specifications = productDto.Specifications,
                ProductCategoryId = productDto.ProductCategoryId,
                ShopId = (int)user.ShopId // Gán sản phẩm cho Shop của người dùng
            };
            await _productRepository.AddAsync(newProduct);
            await _productRepository.SaveChangesAsync();
            return newProduct;
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductDto productDto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || user?.ShopId == null) return false;

            if (product.ShopId != user.ShopId) // Kiểm tra quyền sở hữu
                throw new UnauthorizedAccessException("Bạn không có quyền sửa sản phẩm này.");

            // Map thuộc tính
            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Features = productDto.Features;
            product.ImageUrl = productDto.ImageUrl;
            product.IsPopular = productDto.IsPopular;
            product.BasePrice = productDto.BasePrice;
            product.MaxPrice = productDto.MaxPrice;
            product.StockQuantity = productDto.StockQuantity;
            product.Specifications = productDto.Specifications;
            product.ProductCategoryId = productDto.ProductCategoryId;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || user?.ShopId == null) return false;

            if (product.ShopId != user.ShopId) // Kiểm tra quyền sở hữu
                throw new UnauthorizedAccessException("Bạn không có quyền xóa sản phẩm này.");

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }
    }
}

using Application.Dtos;
using Application.Interfaces;
using Application.Utils;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IImageService _imageService;

        public ProductService(IProductRepository productRepository, IUserRepository userRepository, IImageService imageService)
        {
            _productRepository = productRepository;
            _userRepository = userRepository;
            _imageService = imageService;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDto); // Dùng hàm map chung
        }

        public async Task<IEnumerable<ProductResponseDto>> GetByCategoryIdAsync(int categoryId)
        {
            var products = await _productRepository.GetByCategoryIdAsync(categoryId);
            return products.Select(MapToDto);
        }

        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;
            return MapToDto(product);
        }

        public async Task<Product> CreateAsync(CreateProductDto productDto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.ShopId == null)
                throw new UnauthorizedAccessException("Người dùng này không sở hữu cửa hàng nào.");

            string? specificationsJson = null;
            if (productDto.Specifications != null && productDto.Specifications.Any())
            {
                specificationsJson = JsonSerializer.Serialize(productDto.Specifications);
            }

            var newProduct = new Product
            {
                Name = productDto.Name,
                SearchableName = StringUtils.RemoveAccents(productDto.Name),
                Description = productDto.Description,
                Features = productDto.Features,
                IsPopular = productDto.IsPopular,
                BasePrice = productDto.BasePrice,
                MaxPrice = productDto.MaxPrice,
                StockQuantity = productDto.StockQuantity,
                ProductCategoryId = productDto.ProductCategoryId,
                ShopId = (int)user.ShopId,
                Images = new List<ProductImage>(),
                Specifications = specificationsJson
            };

            if (productDto.ImageFiles != null)
            {
                foreach (var file in productDto.ImageFiles)
                {
                    var imageUrl = await _imageService.SaveImageAsync(file, "products");
                    if (imageUrl != null)
                    {
                        newProduct.Images.Add(new ProductImage { ImageUrl = imageUrl });
                    }
                }
            }
            // 2. Xử lý link URL
            if (productDto.ImageUrls != null)
            {
                foreach (var url in productDto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    newProduct.Images.Add(new ProductImage { ImageUrl = url });
                }
            }

            await _productRepository.AddAsync(newProduct);
            await _productRepository.SaveChangesAsync();
            return newProduct;
        }
        public async Task<bool> UpdateAsync(int id, UpdateProductDto productDto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var product = await _productRepository.GetByIdAsync(id); // Lấy product gốc
            if (product == null || user?.ShopId == null) return false;
            if (product.ShopId != user.ShopId)
                throw new UnauthorizedAccessException("Bạn không có quyền sửa sản phẩm này.");

            string? specificationsJson = null;
            if (productDto.Specifications != null && productDto.Specifications.Any())
            {
                specificationsJson = JsonSerializer.Serialize(productDto.Specifications);
            }

            var existingImageUrls = productDto.ExistingImageUrls ?? new List<string>();

            // 1. Xóa các ảnh cũ mà FE không gửi về trong ExistingImageUrls
            var imagesToDelete = product.Images
                .Where(img => !existingImageUrls.Contains(img.ImageUrl))
                .ToList();

            foreach (var img in imagesToDelete)
            {
                _imageService.DeleteImage(img.ImageUrl); // Xóa file vật lý
                product.Images.Remove(img); // Xóa khỏi collection (EF sẽ xóa khỏi DB)
            }

            // 2. Thêm file upload mới
            if (productDto.ImageFiles != null)
            {
                foreach (var file in productDto.ImageFiles)
                {
                    var imageUrl = await _imageService.SaveImageAsync(file, "products");
                    if (imageUrl != null)
                    {
                        product.Images.Add(new ProductImage { ImageUrl = imageUrl });
                    }
                }
            }
            // 3. Thêm URL mới (chỉ thêm nếu chưa tồn tại)
            if (productDto.ImageUrls != null)
            {
                foreach (var url in productDto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    if (!product.Images.Any(i => i.ImageUrl == url) && !existingImageUrls.Contains(url))
                    {
                        product.Images.Add(new ProductImage { ImageUrl = url });
                    }
                }
            }

            // Map các thuộc tính
            product.Name = productDto.Name;
            product.SearchableName = StringUtils.RemoveAccents(productDto.Name);
            product.Description = productDto.Description;
            product.Features = productDto.Features;
            product.IsPopular = productDto.IsPopular;
            product.BasePrice = productDto.BasePrice;
            product.MaxPrice = productDto.MaxPrice;
            product.StockQuantity = productDto.StockQuantity;
            product.Specifications = specificationsJson;
            product.ProductCategoryId = productDto.ProductCategoryId;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetByShopIdAsync(int shopId)
        {
            var products = await _productRepository.GetByShopIdAsync(shopId);
            return products.Select(MapToDto);
        }

        public async Task<bool> DeleteAsync(int id, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var product = await _productRepository.GetByIdAsync(id); // Lấy product gốc
            if (product == null || user?.ShopId == null) return false;
            if (product.ShopId != user.ShopId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa sản phẩm này.");

            foreach (var img in product.Images)
            {
                _imageService.DeleteImage(img.ImageUrl);
            };
            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }
        private ProductResponseDto MapToDto(Product p)
        {
            if (p == null) return null!;

            // Giải tuần tự hóa (Deserialize) chuỗi JSON Specifications
            Dictionary<string, string>? specificationsDict = null;
            if (!string.IsNullOrWhiteSpace(p.Specifications))
            {
                try
                {
                    // Chuyển chuỗi JSON từ DB về lại Dictionary
                    specificationsDict = JsonSerializer.Deserialize<Dictionary<string, string>>(p.Specifications);
                }
                catch (JsonException)
                {
                    // Xử lý nếu JSON trong DB bị lỗi (ví dụ: trả về rỗng hoặc thông báo lỗi)
                    specificationsDict = new Dictionary<string, string> { { "Lỗi", "Định dạng thông số kỹ thuật không hợp lệ." } };
                }
            }

            return new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Features = p.Features,
                ImageUrls = p.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
                IsPopular = p.IsPopular,
                BasePrice = p.BasePrice,
                MaxPrice = p.MaxPrice,
                StockQuantity = p.StockQuantity,
                Specifications = specificationsDict, // <-- Gán Dictionary đã deserialize
                ProductCategoryId = p.ProductCategoryId,
                ProductCategoryName = p.ProductCategory?.Name ?? "N/A",
                ShopId = p.ShopId,
                ShopName = p.Shop?.Name ?? "N/A"
            };
        }
    }
}


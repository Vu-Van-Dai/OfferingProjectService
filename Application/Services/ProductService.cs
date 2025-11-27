using Application.Dtos;
using Application.Interfaces;
using Application.Utils;
using Core.Entities;
using Microsoft.Extensions.Logging; // ✅ SỬA LỖI #15: Thêm
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
        // ✅ SỬA LỖI #15: Thêm Logger
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, IUserRepository userRepository, IImageService imageService, ILogger<ProductService> logger) // Thêm vào constructor
        {
            _productRepository = productRepository;
            _userRepository = userRepository;
            _imageService = imageService;
            _logger = logger; // Thêm
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

        public async Task<Product> CreateAsync(CreateProductDto dto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.Shop == null)
                throw new UnauthorizedAccessException("Người dùng không có Shop.");

            var product = new Product
            {
                Name = dto.Name,
                SearchableName = StringUtils.RemoveAccents(dto.Name),
                Description = dto.Description,
                Features = dto.Features,
                IsPopular = dto.IsPopular,
                BasePrice = dto.BasePrice,
                MaxPrice = dto.MaxPrice,
                StockQuantity = dto.StockQuantity,
                ProductCategoryId = dto.ProductCategoryId,
                ShopId = user.Shop.Id,
                Specifications = dto.Specifications != null ? JsonSerializer.Serialize(dto.Specifications) : null,
                Images = new List<ProductImage>()
            };

            // Xử lý ảnh
            if (dto.ImageFiles != null)
            {
                foreach (var file in dto.ImageFiles)
                {
                    try
                    {
                        var (data, mime) = await _imageService.ProcessImageAsync(file);
                        product.Images.Add(new ProductImage { ImageData = data, ImageMimeType = mime });
                    }
                    catch (Exception ex) { _logger.LogWarning(ex, "Lỗi xử lý ảnh khi tạo sản phẩm."); }
                }
            }
            if (dto.ImageUrls != null)
            {
                foreach (var urlStr in dto.ImageUrls)
                {
                    var result = await _imageService.ProcessStringImageAsync(urlStr);
                    if (result != null)
                    {
                        product.Images.Add(new ProductImage
                        {
                            ImageData = result.Value.Data,
                            ImageMimeType = result.Value.MimeType
                        });
                    }
                }
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
            return product;
        }
        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null || user?.Shop == null || product.ShopId != user.Shop.Id)
                return false; // Hoặc throw Exception tùy logic

            // Update thông tin cơ bản
            product.Name = dto.Name;
            product.SearchableName = StringUtils.RemoveAccents(dto.Name);
            product.Description = dto.Description;
            product.Features = dto.Features;
            product.IsPopular = dto.IsPopular;
            product.BasePrice = dto.BasePrice;
            product.MaxPrice = dto.MaxPrice;
            product.StockQuantity = dto.StockQuantity;
            product.ProductCategoryId = dto.ProductCategoryId;
            product.Specifications = dto.Specifications != null ? JsonSerializer.Serialize(dto.Specifications) : null;

            // Xử lý ảnh:
            // 1. Giữ lại ảnh cũ nếu ID có trong KeepImageIds
            var keepIds = dto.KeepImageIds ?? new List<int>();
            var imagesToRemove = product.Images.Where(i => !keepIds.Contains(i.Id)).ToList();
            foreach (var img in imagesToRemove)
            {
                product.Images.Remove(img); // EF sẽ tự delete do cascade
            }

            // 2. Thêm ảnh mới
            if (dto.ImageFiles != null)
            {
                foreach (var file in dto.ImageFiles)
                {
                    var (data, mime) = await _imageService.ProcessImageAsync(file);
                    product.Images.Add(new ProductImage { ImageData = data, ImageMimeType = mime });
                }
            }

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
            // Logic delete giữ nguyên, EF Cascade sẽ tự xóa ProductImage trong DB
            var user = await _userRepository.GetByIdAsync(userId);
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null || user?.Shop == null || product.ShopId != user.Shop.Id)
                throw new UnauthorizedAccessException("Không có quyền xóa.");

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }
        private ProductResponseDto MapToDto(Product p)
        {
            Dictionary<string, string> specs = new();
            try { if (!string.IsNullOrEmpty(p.Specifications)) specs = JsonSerializer.Deserialize<Dictionary<string, string>>(p.Specifications) ?? new(); }
            catch { }

            return new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Features = p.Features,
                IsPopular = p.IsPopular,
                BasePrice = p.BasePrice,
                MaxPrice = p.MaxPrice,
                StockQuantity = p.StockQuantity,
                Specifications = specs,
                ProductCategoryId = p.ProductCategoryId,
                ProductCategoryName = p.ProductCategory?.Name ?? "N/A",
                ShopId = p.ShopId,
                ShopName = p.Shop?.Name ?? "N/A",
                // Convert byte[] sang Base64
                Images = p.Images?.Select(i => new ImageDto 
                { 
                    Id = i.Id, 
                    Url = _imageService.ToBase64(i.ImageData, i.ImageMimeType) 
                }).ToList() ?? new List<ImageDto>()
            };
        }
    }
}
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
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IActivityLogService _logService; // Inject dịch vụ Log
        private readonly IImageService _imageService;

        public AdminCategoryService(ICategoryRepository categoryRepo, IActivityLogService logService, IImageService imageService)
        {
            _categoryRepo = categoryRepo;
            _logService = logService;
            _imageService = imageService;
        }

        public async Task<IEnumerable<AdminCategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllWithProductCountAsync();
            return categories.Select(c => new AdminCategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                IsHidden = c.IsHidden,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count
            });
        }

        public async Task<ProductCategory> CreateCategoryAsync(CreateCategoryDto dto)
        {
            byte[]? iconData = null;
            string? iconMime = null;

            // 1. Ưu tiên File Upload
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var result = await _imageService.ProcessImageAsync(dto.ImageFile);
                iconData = result.Data;
                iconMime = result.MimeType;
            }
            // 2. Nếu không có file, kiểm tra URL (nếu bạn thêm ImageUrl vào DTO)
            else if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                var result = await _imageService.ProcessStringImageAsync(dto.ImageUrl);
                if (result != null)
                {
                    iconData = result.Value.Data;
                    iconMime = result.Value.MimeType;
                }
            }

            var newCategory = new ProductCategory
            {
                Name = dto.Name,
                BannerTitle = dto.BannerTitle,
                Description = dto.Description,
                // Lưu byte[]
                IconData = iconData,
                IconMimeType = iconMime,
                IsHidden = false,
                DisplayOrder = (await _categoryRepo.GetAllAsync()).Count()
            };

            await _categoryRepo.AddAsync(newCategory);
            await _categoryRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Tạo Danh Mục", $"Đã tạo: {dto.Name}");
            return newCategory;
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return false;

            // Xử lý ảnh:
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                // Có ảnh mới -> Ghi đè
                var result = await _imageService.ProcessImageAsync(dto.ImageFile);
                category.IconData = result.Data;
                category.IconMimeType = result.MimeType;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                // Có URL mới -> Tải và ghi đè
                var result = await _imageService.ProcessStringImageAsync(dto.ImageUrl);
                if (result != null)
                {
                    category.IconData = result.Value.Data;
                    category.IconMimeType = result.Value.MimeType;
                }
            }
            // Lưu ý: Nếu không gửi gì cả thì GIỮ NGUYÊN ảnh cũ (không xóa).
            // Nếu muốn xóa ảnh, cần thêm 1 trường cờ (flag) từ FE gửi lên.

            category.Name = dto.Name;
            category.BannerTitle = dto.BannerTitle;
            category.Description = dto.Description;

            _categoryRepo.Update(category);
            await _categoryRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Sửa Danh Mục", $"Đã sửa ID: {id}");
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return false;

            _categoryRepo.Delete(category);
            await _categoryRepo.SaveChangesAsync(); // Ảnh trong DB sẽ tự mất cùng dòng dữ liệu

            await _logService.LogAsync("Admin", "Xóa Danh Mục", $"Đã xóa: {category.Name}");
            return true;
        }

        public async Task<bool> ToggleCategoryVisibilityAsync(int id, bool isHidden)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return false;

            category.IsHidden = isHidden;
            _categoryRepo.Update(category);
            await _categoryRepo.SaveChangesAsync();

            var action = isHidden ? "Ẩn" : "Hiện";
            await _logService.LogAsync("Admin", "Ẩn/Hiện Danh Mục", $"{action} danh mục: {category.Name}");
            return true;
        }

        public async Task<bool> ReorderCategoriesAsync(List<int> orderedCategoryIds)
        {
            var categories = await _categoryRepo.GetAllAsync(); // Lấy tất cả
            if (!categories.Any()) return false;

            int order = 0;
            foreach (var id in orderedCategoryIds)
            {
                var category = categories.FirstOrDefault(c => c.Id == id);
                if (category != null)
                {
                    category.DisplayOrder = order;
                    _categoryRepo.Update(category);
                    order++;
                }
            }

            await _categoryRepo.SaveChangesAsync();
            await _logService.LogAsync("Admin", "Sắp xếp Danh Mục", $"Đã cập nhật thứ tự danh mục");
            return true;
        }
    }
}

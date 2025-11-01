﻿using Application.Dtos;
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

        public AdminCategoryService(ICategoryRepository categoryRepo, IActivityLogService logService)
        {
            _categoryRepo = categoryRepo;
            _logService = logService;
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
            var newCategory = new ProductCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsHidden = false,
                DisplayOrder = (await _categoryRepo.GetAllAsync()).Count() // Tự động xếp cuối
            };

            await _categoryRepo.AddAsync(newCategory);
            await _categoryRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Tạo Danh Mục", $"Đã tạo danh mục mới: {dto.Name}");
            return newCategory;
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return false;

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl;

            _categoryRepo.Update(category);
            await _categoryRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Sửa Danh Mục", $"Đã sửa danh mục ID: {id}");
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return false;

            // Cân nhắc: Chỉ nên cho xóa nếu không có sản phẩm nào
            // if (category.Products.Any()) return false; 

            _categoryRepo.Delete(category);
            await _categoryRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Xóa Danh Mục", $"Đã xóa danh mục: {category.Name} (ID: {id})");
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

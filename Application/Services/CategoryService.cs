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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategorySummaryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            // Map sang DTO
            return categories.Select(c => new CategorySummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description, // Thêm Description
                ImageUrl = c.ImageUrl,
                ProductCount = c.Products.Count // Lấy số lượng SP từ collection đã Include
            });
        }

        public async Task<ProductCategory> CreateAsync(CreateCategoryDto categoryDto)
        {
            var newCategory = new ProductCategory
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ImageUrl = categoryDto.ImageUrl
            };
            await _categoryRepository.AddAsync(newCategory);
            await _categoryRepository.SaveChangesAsync();
            return newCategory;
        }
        public async Task<bool> UpdateAsync(int id, UpdateCategoryDto categoryDto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null) return false;

            // Cập nhật thuộc tính
            existingCategory.Name = categoryDto.Name;
            existingCategory.Description = categoryDto.Description;
            existingCategory.ImageUrl = categoryDto.ImageUrl;

            _categoryRepository.Update(existingCategory);
            await _categoryRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();
            return true;
        }
    }
}

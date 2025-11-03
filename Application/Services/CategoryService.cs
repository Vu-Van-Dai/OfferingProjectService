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
        private readonly ICategoryRepository _categoryRepo;

        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<IEnumerable<CategorySummaryDto>> GetAllSummariesAsync()
        {
            var categories = await _categoryRepo.GetAllWithProductCountAsync();
            return categories.Select(c => new CategorySummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl, // Icon
                ProductCount = c.Products.Count
            });
        }

        // Hàm này lấy chi tiết (gồm BannerTitle)
        public async Task<CategoryDetailDto?> GetCategoryDetailsByIdAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryDetailDto
            {
                Id = category.Id,
                Name = category.Name,
                BannerTitle = category.BannerTitle, // Trả về tiêu đề banner
                Description = category.Description, // Trả về mô tả
                ImageUrl = category.ImageUrl // Trả về icon
            };
        }
    }
}

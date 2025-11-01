using Application.Dtos;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAdminCategoryService
    {
        Task<IEnumerable<AdminCategoryResponseDto>> GetAllCategoriesAsync();
        Task<ProductCategory> CreateCategoryAsync(CreateCategoryDto dto);
        Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> ToggleCategoryVisibilityAsync(int id, bool isHidden);
        Task<bool> ReorderCategoriesAsync(List<int> orderedCategoryIds);
    }
}

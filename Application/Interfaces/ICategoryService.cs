using Application.Dtos;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        // Lấy tất cả danh mục (cho trang "Danh Mục Sản Phẩm")
        Task<IEnumerable<CategorySummaryDto>> GetAllAsync();

        // Tạo danh mục mới (cho Admin)
        Task<ProductCategory> CreateAsync(CreateCategoryDto categoryDto);
        Task<bool> UpdateAsync(int id, UpdateCategoryDto categoryDto); // Thêm
        Task<bool> DeleteAsync(int id); // Thêm
    }
}

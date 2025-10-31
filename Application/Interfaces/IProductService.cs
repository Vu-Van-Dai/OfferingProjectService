using Application.Dtos;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductService
    {
        // Lấy tất cả sản phẩm (cho trang "Tất Cả Sản Phẩm")
        Task<IEnumerable<ProductResponseDto>> GetAllAsync();

        // Lấy sản phẩm theo ID danh mục (cho trang "Hoa Tươi", "Hương Nến"...)
        Task<IEnumerable<ProductResponseDto>> GetByCategoryIdAsync(int categoryId);

        // Lấy chi tiết 1 sản phẩm
        Task<ProductResponseDto?> GetByIdAsync(int id);

        // Tạo sản phẩm mới (cho Admin)
        Task<IEnumerable<ProductResponseDto>> GetByShopIdAsync(int shopId);
        Task<Product> CreateAsync(CreateProductDto productDto, Guid userId); // Thêm userId
        Task<bool> UpdateAsync(int id, UpdateProductDto productDto, Guid userId); // Thêm userId
        Task<bool> DeleteAsync(int id, Guid userId); // Thêm userId
    }
}

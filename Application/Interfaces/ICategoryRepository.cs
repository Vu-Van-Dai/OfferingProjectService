using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<ProductCategory?> GetByIdAsync(int id);
        Task<IEnumerable<ProductCategory>> GetAllAsync();
        Task AddAsync(ProductCategory category);
        void Update(ProductCategory category); // Dùng cho Admin
        void Delete(ProductCategory category); // Dùng cho Admin
        Task<int> SaveChangesAsync(); // Thêm hàm này
    }
}

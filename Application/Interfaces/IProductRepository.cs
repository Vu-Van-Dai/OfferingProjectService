using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
        Task AddAsync(Product product);
        void Update(Product product); // Dùng cho Admin
        void Delete(Product product); // Dùng cho Admin
        Task<int> SaveChangesAsync(); // Thêm hàm này
        Task<IEnumerable<Product>> GetByShopIdAsync(int shopId);
    }
}

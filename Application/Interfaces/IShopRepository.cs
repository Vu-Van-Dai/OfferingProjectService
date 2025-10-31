using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IShopRepository
    {
        Task AddAsync(Shop shop);
        Task<int> SaveChangesAsync();
        Task<IEnumerable<Shop>> SearchByNameAsync(string query);
        Task<Shop?> GetByOwnerUserIdAsync(Guid ownerUserId);
        Task<Shop?> GetByIdAsync(int id); // Có thể cần nếu bạn muốn lấy shop theo ID
        void Update(Shop shop);
    }
}

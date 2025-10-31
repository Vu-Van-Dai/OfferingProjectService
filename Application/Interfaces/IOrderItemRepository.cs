using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<OrderItem?> GetByIdAsync(int id);
        Task<IEnumerable<OrderItem>> GetByShopIdAsync(int shopId, OrderItemShopStatus? status = null);
        Task<int> CountPendingByShopIdAsync(int shopId);
        Task<int> SaveChangesAsync();
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        // Có thể thêm các hàm lấy đơn hàng sau này
        Task<int> SaveChangesAsync();
        Task<Order?> GetByIdAndUserIdAsync(int orderId, Guid userId);

        /// <summary>
        /// Lấy tất cả đơn hàng của một UserId (cho trang Lịch sử).
        /// </summary>
        Task<IEnumerable<Order>> GetListByUserIdAsync(Guid userId);
    }
}

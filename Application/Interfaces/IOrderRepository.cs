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
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    /// <summary>
    /// Repository chỉ dành cho việc truy vấn dữ liệu Order phức tạp (không thay đổi dữ liệu).
    /// </summary>
    public interface IOrderQueryRepository
    {
        Task<List<Order>> GetCompletedOrdersForShopByMonthAsync(int shopId, DateTime startOfMonth);
        Task<Dictionary<DateTime, decimal>> GetDailyRevenueForShopLastDaysAsync(int shopId, DateTime startDate);
    }
}

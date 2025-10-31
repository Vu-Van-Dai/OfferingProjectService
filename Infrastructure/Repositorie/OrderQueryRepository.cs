using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositorie
{
    public class OrderQueryRepository : IOrderQueryRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderQueryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetCompletedOrdersForShopByMonthAsync(int shopId, DateTime startOfMonth)
        {
            // Lấy các đơn hàng HOÀN THÀNH trong tháng của shop
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed &&
                             o.OrderDate >= startOfMonth &&
                             o.Items.Any(i => i.ShopId == shopId))
                .Include(o => o.Items.Where(i => i.ShopId == shopId)) // Chỉ lấy items của shop này
                .ToListAsync();
        }

        public async Task<Dictionary<DateTime, decimal>> GetDailyRevenueForShopLastDaysAsync(int shopId, DateTime startDate)
        {
            // Lấy doanh thu hàng ngày từ ngày startDate đến hiện tại
            return await _context.OrderItems
                .Where(oi => oi.ShopId == shopId &&
                             oi.Order.Status == OrderStatus.Completed &&
                             oi.Order.OrderDate >= startDate)
                .GroupBy(oi => oi.Order.OrderDate.Date) // Nhóm theo ngày
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(i => i.Price * i.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToDictionaryAsync(x => x.Date, x => x.Revenue);
        }
    }
}

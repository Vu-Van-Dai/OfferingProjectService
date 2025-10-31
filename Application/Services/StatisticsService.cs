using Application.Dtos.Application.Dtos;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IOrderQueryRepository _orderQueryRepository; // Sửa: Dùng Repo mới

        public StatisticsService(IOrderQueryRepository orderQueryRepository) // Sửa constructor
        {
            _orderQueryRepository = orderQueryRepository;
        }

        public async Task<ShopStatisticsDto> GetShopStatisticsAsync(int shopId)
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var today = now.Date;
            var thirtyDaysAgo = now.AddDays(-30).Date;

            // Lấy đơn hàng hoàn thành trong tháng bằng Repository
            var completedOrdersThisMonth = await _orderQueryRepository.GetCompletedOrdersForShopByMonthAsync(shopId, startOfMonth);

            var revenueThisMonth = completedOrdersThisMonth
                .SelectMany(o => o.Items) // Items đã được lọc theo shopId bởi Repo
                .Sum(i => i.Price * i.Quantity);

            var ordersTodayCount = completedOrdersThisMonth.Count(o => o.OrderDate.Date == today);

            // Lấy doanh thu hàng ngày bằng Repository
            var dailyRevenueData = await _orderQueryRepository.GetDailyRevenueForShopLastDaysAsync(shopId, thirtyDaysAgo);

            // Điền các ngày không có doanh thu bằng 0
            var fullDailyRevenue = new Dictionary<DateTime, decimal>();
            for (var date = thirtyDaysAgo; date <= today; date = date.AddDays(1))
            {
                fullDailyRevenue[date] = dailyRevenueData.GetValueOrDefault(date, 0);
            }

            return new ShopStatisticsDto
            {
                RevenueThisMonth = revenueThisMonth,
                OrdersThisMonth = completedOrdersThisMonth.Count,
                OrdersToday = ordersTodayCount, // Đã thêm vào DTO
                DailyRevenueLast30Days = fullDailyRevenue
            };
        }
    }
}

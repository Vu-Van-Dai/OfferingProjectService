using Application.Dtos;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IOrderQueryRepository _orderQueryRepo;

        public AdminDashboardService(IOrderQueryRepository orderQueryRepo)
        {
            _orderQueryRepo = orderQueryRepo;
        }

        public async Task<AdminDashboardStatsDto> GetRevenueStatsAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)now.DayOfWeek); // Giả sử tuần bắt đầu Chủ nhật
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);
            var tomorrow = today.AddDays(1);

            // Chạy các truy vấn song song
            var revenueTodayTask = _orderQueryRepo.GetTotalRevenueAsync(today, tomorrow);
            var revenueThisWeekTask = _orderQueryRepo.GetTotalRevenueAsync(startOfWeek, tomorrow);
            var revenueThisMonthTask = _orderQueryRepo.GetTotalRevenueAsync(startOfMonth, tomorrow);
            var revenueThisYearTask = _orderQueryRepo.GetTotalRevenueAsync(startOfYear, tomorrow);

            await Task.WhenAll(revenueTodayTask, revenueThisWeekTask, revenueThisMonthTask, revenueThisYearTask);

            return new AdminDashboardStatsDto
            {
                RevenueToday = await revenueTodayTask,
                RevenueThisWeek = await revenueThisWeekTask,
                RevenueThisMonth = await revenueThisMonthTask,
                RevenueThisYear = await revenueThisYearTask
            };
        }

        public async Task<IEnumerable<AdminShopRevenueDto>> GetRevenueByShopAsync()
        {
            // Lấy doanh thu của tất cả shop từ trước đến nay
            // Bạn có thể thêm tham số (DateTime start, DateTime end) nếu muốn lọc
            var allTimeStart = DateTime.MinValue;
            var now = DateTime.UtcNow;

            return await _orderQueryRepo.GetTotalRevenueByShopAsync(allTimeStart, now);
        }
    }
}

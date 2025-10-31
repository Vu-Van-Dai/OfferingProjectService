using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    namespace Application.Dtos
    {
        public class ShopDashboardDto
        {
            public int TotalProducts { get; set; }
            public int ProductsInStock { get; set; }
            public int OutOfStockProducts { get; set; }
            public int PendingOrderItems { get; set; }
        }

        // DTO cơ bản cho Thống kê
        public class ShopStatisticsDto
        {
            public decimal RevenueThisMonth { get; set; }
            public int OrdersThisMonth { get; set; }
            public Dictionary<DateTime, decimal> DailyRevenueLast30Days { get; set; } = new Dictionary<DateTime, decimal>();
            public int OrdersToday { get; set; }
        }
    }
}

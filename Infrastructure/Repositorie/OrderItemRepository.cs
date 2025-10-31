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
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderItem?> GetByIdAsync(int id)
        {
            // Include các thông tin liên quan cần thiết
            return await _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.ShippingAddress) // Lấy địa chỉ giao hàng
                .Include(oi => oi.ProductOrdered) // Lấy tên sản phẩm
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<IEnumerable<OrderItem>> GetByShopIdAsync(int shopId, OrderItemShopStatus? status = null)
        {
            var query = _context.OrderItems
                .Where(oi => oi.ShopId == shopId)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.ShippingAddress) // Include địa chỉ
                .Include(oi => oi.ProductOrdered) // Include sản phẩm
                .OrderByDescending(oi => oi.Order.OrderDate) // Sắp xếp theo ngày đặt hàng mới nhất
                as IQueryable<OrderItem>;

            if (status.HasValue)
            {
                query = query.Where(oi => oi.ShopStatus == status.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> CountPendingByShopIdAsync(int shopId)
        {
            return await _context.OrderItems.CountAsync(oi => oi.ShopId == shopId && oi.ShopStatus == OrderItemShopStatus.Pending);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

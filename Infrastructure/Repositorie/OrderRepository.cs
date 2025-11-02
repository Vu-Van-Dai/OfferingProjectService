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
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context) { _context = context; }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<Order?> GetByIdAndUserIdAsync(int orderId, Guid userId)
        {
            return await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.ProductOrdered)
                        .ThenInclude(p => p.Images) // Lấy ảnh sản phẩm
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Shop) // Lấy tên shop
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerUserId == userId); // Kiểm tra quyền sở hữu
        }

        public async Task<IEnumerable<Order>> GetListByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.BuyerUserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.ProductOrdered)
                        .ThenInclude(p => p.Images) // Cần ảnh đầu tiên
                .OrderByDescending(o => o.OrderDate) // Đơn mới nhất lên đầu
                .ToListAsync();
        }
    }
}

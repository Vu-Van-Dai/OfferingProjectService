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
    public class ShopRepository : IShopRepository
    {
        private readonly ApplicationDbContext _context;
        public ShopRepository(ApplicationDbContext context) { _context = context; }
        public async Task AddAsync(Shop shop)
        {
            await _context.Shops.AddAsync(shop);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Shop>> SearchByNameAsync(string normalizedQuery)
        {
            // Tìm trên cột SearchableName
            return await _context.Shops
                .Where(s => s.SearchableName.Contains(normalizedQuery))
                .Include(s => s.Products.Where(p => p.IsPopular).Take(3))
                .ToListAsync();
        }
        public async Task<Shop?> GetByOwnerUserIdAsync(Guid ownerUserId)
        {
            // Include Products nếu bạn muốn hiển thị SP trên trang profile shop
            return await _context.Shops
                // .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.OwnerUserId == ownerUserId);
        }

        public async Task<Shop?> GetByIdAsync(int id)
        {
            return await _context.Shops.FindAsync(id);
        }

        public void Update(Shop shop)
        {
            _context.Shops.Update(shop);
        }
        public async Task<List<Shop>> GetAllWithOwnersAsync()
        {
            return await _context.Shops
                .Include(s => s.OwnerUser) // Include thông tin chủ shop
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Shop?> GetByIdWithOwnerAsync(int id)
        {
            return await _context.Shops
                .Include(s => s.OwnerUser) // Include thông tin chủ shop
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}

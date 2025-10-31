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
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.Shop)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.ProductCategoryId == categoryId)
                .Include(p => p.ProductCategory)
                .Include(p => p.Shop)
                .ToListAsync();
        }
        public async Task<Product?> GetByIdAsync(int id)
        {
            // Tải kèm Shop và Reviews khi lấy chi tiết
            return await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.Shop)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetByShopIdAsync(int shopId)
        {
            return await _context.Products
                .Where(p => p.ShopId == shopId)
                .Include(p => p.ProductCategory)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string normalizedQuery)
        {
            // Bây giờ chúng ta tìm trên cột SearchableName
            return await _context.Products
                .Where(p => p.SearchableName.Contains(normalizedQuery))
                .Include(p => p.Shop)
                .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public void Delete(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<int> CountByShopIdAsync(int shopId)
        {
            return await _context.Products.CountAsync(p => p.ShopId == shopId);
        }

        public async Task<int> CountInStockByShopIdAsync(int shopId)
        {
            return await _context.Products.CountAsync(p => p.ShopId == shopId && p.StockQuantity > 0);
        }

        public async Task<int> CountOutOfStockByShopIdAsync(int shopId)
        {
            return await _context.Products.CountAsync(p => p.ShopId == shopId && p.StockQuantity <= 0);
        }
    }
}

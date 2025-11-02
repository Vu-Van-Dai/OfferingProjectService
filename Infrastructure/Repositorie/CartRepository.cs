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
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        public CartRepository(ApplicationDbContext context) { _context = context; }

        public async Task<Cart?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images)// Include Product để lấy giá, tên, ảnh
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Shop) // Include Shop để nhóm
                .FirstOrDefaultAsync(c => c.OwnerUserId == userId);
        }

        public async Task<Cart> CreateForUserAsync(Guid userId)
        {
            var newCart = new Cart { OwnerUserId = userId };
            await _context.Carts.AddAsync(newCart);
            // Chưa cần SaveChangesAsync ở đây, Service sẽ gọi sau
            return newCart;
        }

        public async Task<CartItem?> GetItemInCartAsync(int cartId, int productId)
        {
            return await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
        }

        public async Task AddItemAsync(CartItem item)
        {
            await _context.CartItems.AddAsync(item);
        }

        public void RemoveItem(CartItem item)
        {
            _context.CartItems.Remove(item);
        }

        public async Task<CartItem?> GetItemByIdAsync(int cartItemId)
        {
            return await _context.CartItems.FindAsync(cartItemId);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

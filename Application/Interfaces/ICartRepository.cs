using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task<Cart> CreateForUserAsync(Guid userId);
        Task<CartItem?> GetItemInCartAsync(int cartId, int productId);
        Task AddItemAsync(CartItem item);
        void RemoveItem(CartItem item);
        Task<CartItem?> GetItemByIdAsync(int cartItemId);
        Task<int> SaveChangesAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Dtos.CartDtos;

namespace Application.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(Guid userId);
        Task<CartResponseDto> AddItemToCartAsync(Guid userId, AddItemToCartDto itemDto);
        Task<CartResponseDto> UpdateItemQuantityAsync(Guid userId, int cartItemId, int quantity);
        Task<CartResponseDto> RemoveItemAsync(Guid userId, int cartItemId);
        Task<CartResponseDto> SetItemSelectionAsync(Guid userId, int cartItemId, bool isSelected);
    }
}

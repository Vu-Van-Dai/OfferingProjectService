using Application.Dtos;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderItemService
    {
        Task<IEnumerable<ShopOrderItemDto>> GetOrderItemsForShopAsync(int shopId, OrderItemShopStatus? status);
        Task<bool> UpdateOrderItemStatusAsync(int shopId, int orderItemId, OrderItemShopStatus newStatus);
    }
}

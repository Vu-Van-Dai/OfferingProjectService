using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderItemService(IOrderItemRepository orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
        }

        public async Task<IEnumerable<ShopOrderItemDto>> GetOrderItemsForShopAsync(int shopId, OrderItemShopStatus? status)
        {
            var items = await _orderItemRepository.GetByShopIdAsync(shopId, status);

            // Map sang DTO
            return items.Select(oi => new ShopOrderItemDto
            {
                OrderItemId = oi.Id,
                OrderId = oi.OrderId,
                OrderDate = oi.Order.OrderDate,
                ProductName = oi.ProductOrdered.Name, // Đảm bảo Repo đã Include ProductOrdered
                Quantity = oi.Quantity,
                Price = oi.Price,
                CurrentShopStatus = oi.ShopStatus.ToString(),
                BuyerName = oi.Order.ShippingAddress.FullName, // Đảm bảo Repo đã Include Order -> ShippingAddress
                BuyerPhoneNumber = oi.Order.ShippingAddress.PhoneNumber,
                ShippingAddress = oi.Order.ShippingAddress
            });
        }

        public async Task<bool> UpdateOrderItemStatusAsync(int shopId, int orderItemId, OrderItemShopStatus newStatus)
        {
            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId);

            // Kiểm tra xem OrderItem có tồn tại và thuộc về đúng Shop không
            if (orderItem == null || orderItem.ShopId != shopId)
            {
                return false;
            }

            orderItem.ShopStatus = newStatus;
            // Lưu ý: Có thể cần thêm logic kiểm tra chuyển đổi trạng thái hợp lệ
            // Ví dụ: Không thể chuyển từ Shipped về Pending

            await _orderItemRepository.SaveChangesAsync();
            return true;
        }
    }
}

﻿using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserAddressRepository _addressRepository;

        public OrderService(ICartRepository cartRepository, IOrderRepository orderRepository, IUserAddressRepository addressRepository)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _addressRepository = addressRepository;
        }

        public async Task<OrderResponseDto?> CreateOrderFromCartAsync(Guid userId, CreateOrderRequestDto orderRequest)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any(i => i.IsSelected)) return null;

            // 1. Xác định địa chỉ giao hàng
            ShippingAddress? addressToUse = null;
            if (orderRequest.ShippingAddress != null)
            {
                // Người dùng gửi lên địa chỉ MỚI (không lưu vào sổ)
                addressToUse = orderRequest.ShippingAddress;
            }
            else
            {
                // Lấy địa chỉ MẶC ĐỊNH từ sổ địa chỉ
                var defaultUserAddress = await _addressRepository.GetDefaultForUserAsync(userId);
                if (defaultUserAddress != null)
                {
                    // Map từ UserAddress sang ShippingAddress (chúng có cấu trúc giống nhau)
                    addressToUse = new ShippingAddress
                    {
                        FullName = defaultUserAddress.FullName,
                        PhoneNumber = defaultUserAddress.PhoneNumber,
                        Street = defaultUserAddress.Street,
                        Ward = defaultUserAddress.Ward,
                        District = defaultUserAddress.District,
                        City = defaultUserAddress.City
                    };
                }
            }

            if (addressToUse == null)
            {
                // Nếu không có địa chỉ nào -> yêu cầu FE nhập
                throw new Exception("Vui lòng cung cấp địa chỉ giao hàng.");
            }


            // 2. Lọc các món hàng đã chọn
            var selectedItems = cart.Items.Where(i => i.IsSelected).ToList();

            // 3. Tính toán tổng tiền
            var subtotal = selectedItems.Sum(item => item.Product.BasePrice * item.Quantity);
            // Bạn có thể thêm logic tính phí ship, voucher ở đây
            var total = subtotal; // Tạm thời total = subtotal

            // 4. Tạo đối tượng Order
            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending, // Chờ thanh toán
                Subtotal = subtotal,
                Total = total,
                ShippingAddress = addressToUse,
                BuyerUserId = userId
            };

            // 5. Tạo các OrderItem tương ứng
            foreach (var item in selectedItems)
            {
                // Có thể thêm logic kiểm tra tồn kho ở đây
                // if (item.Product.StockQuantity < item.Quantity) throw ...

                var orderItem = new OrderItem
                {
                    Quantity = item.Quantity,
                    Price = item.Product.BasePrice, // Lưu giá tại thời điểm mua
                    ProductId = item.ProductId,
                    ShopId = item.ShopId,
                    Order = order // Tự động gán OrderId
                };
                order.Items.Add(orderItem);

                // Giảm số lượng tồn kho (nếu cần)
                // item.Product.StockQuantity -= item.Quantity;
            }

            // 6. Lưu Order và OrderItems vào database
            await _orderRepository.AddAsync(order);

            // 7. Xóa các món hàng đã chọn khỏi giỏ
            foreach (var item in selectedItems)
            {
                _cartRepository.RemoveItem(item);
            }

            // 8. Lưu tất cả thay đổi (Order mới, CartItem bị xóa, Product Stock giảm)
            await _orderRepository.SaveChangesAsync(); // Hoặc _cartRepository.SaveChangesAsync();

            // 9. Gửi Email (Sẽ làm sau)
            // if(user.Email != null) {
            //    await _emailService.SendOrderConfirmationEmailAsync(user.Email, order);
            // }

            // 10. Map sang DTO để trả về
            return MapOrderToDto(order);
        }

        // Hàm map sang DTO (tương tự MapCartToDto)
        private OrderResponseDto MapOrderToDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                Subtotal = order.Subtotal,
                Total = order.Total,
                ShippingAddress = order.ShippingAddress,
                Items = order.Items.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductOrdered.Name,
                    ImageUrl = oi.ProductOrdered.ImageUrl,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    ShopName = oi.Shop.Name
                }).ToList()
            };
        }
    }
}

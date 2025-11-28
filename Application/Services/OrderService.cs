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
    public class OrderService : IOrderService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserAddressRepository _addressRepository;
        private readonly IImageService _imageService;

        public OrderService(ICartRepository cartRepository, IOrderRepository orderRepository, IUserAddressRepository addressRepository, IImageService imageService)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _addressRepository = addressRepository;
            _imageService = imageService;
        }

        public async Task<OrderResponseDto?> CreateOrderFromCartAsync(Guid userId, CreateOrderRequestDto orderRequest)
        {
            // ✅ SỬA LỖI #14: Cần tải lại cart với Product (vì GetByUserIdAsync đã làm)
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
                // ✅ SỬA LỖI #14: Kiểm tra tồn kho
                if (item.Product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm '{item.Product.Name}' không đủ hàng. " +
                        $"Còn lại: {item.Product.StockQuantity}, yêu cầu: {item.Quantity}"
                    );
                }

                var orderItem = new OrderItem
                {
                    Quantity = item.Quantity,
                    Price = item.Product.BasePrice, // Lưu giá tại thời điểm mua
                    ProductId = item.ProductId,
                    ShopId = item.ShopId,
                    Order = order // Tự động gán OrderId
                };
                order.Items.Add(orderItem);

                // ✅ SỬA LỖI #14: Giảm số lượng tồn kho
                item.Product.StockQuantity -= item.Quantity;
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
        public async Task<OrderResponseDto?> GetOrderDetailsAsync(Guid userId, int orderId)
        {
            var order = await _orderRepository.GetByIdAndUserIdAsync(orderId, userId);

            if (order == null)
            {
                return null; // Không tìm thấy hoặc không có quyền xem
            }

            return MapOrderToDto(order); // Dùng lại hàm map cũ
        }

        public async Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(Guid userId)
        {
            var orders = await _orderRepository.GetListByUserIdAsync(userId);

            return orders.Select(o =>
            {
                var firstItemProduct = o.Items.FirstOrDefault()?.ProductOrdered;
                var firstImg = firstItemProduct?.Images.FirstOrDefault();

                return new OrderHistoryDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    Total = o.Total,
                    TotalItems = o.Items.Count,
                    PrimaryProductName = firstItemProduct?.Name ?? "N/A",
                    // SỬA LỖI TẠI ĐÂY
                    PrimaryProductImage = firstImg != null ? _imageService.ToBase64(firstImg.ImageData, firstImg.ImageMimeType) : null
                };
            });
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
                Items = order.Items.Select(oi =>
                {
                    var img = oi.ProductOrdered.Images?.FirstOrDefault();
                    return new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductOrdered.Name,
                        // SỬA LỖI TẠI ĐÂY
                        ImageUrl = img != null ? _imageService.ToBase64(img.ImageData, img.ImageMimeType) : null,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                        ShopName = oi.Shop.Name
                    };
                }).ToList()
            };
        }
    }
}
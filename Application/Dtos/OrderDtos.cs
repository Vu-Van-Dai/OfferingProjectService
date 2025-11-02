using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    // DTO Frontend gửi lên khi muốn tạo đơn hàng
    // (Chỉ cần nếu địa chỉ chưa có sẵn)
    public class CreateOrderRequestDto
    {
        // Nếu user đã có địa chỉ trong profile thì không cần gửi cái này
        public ShippingAddress? ShippingAddress { get; set; }
    }

    // DTO chi tiết của OrderItem trả về
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ShopName { get; set; }
    }

    // DTO trả về thông tin đơn hàng đã tạo
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } // Chuyển Enum thành string
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
    public class ShopOrderItemDto
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string CurrentShopStatus { get; set; } = string.Empty;
        // Thông tin người mua để shop liên hệ
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerPhoneNumber { get; set; } = string.Empty;
        public ShippingAddress ShippingAddress { get; set; } = null!; // Không bao giờ null
    }

    public class UpdateOrderItemStatusDto
    {
        [Required]
        [EnumDataType(typeof(OrderItemShopStatus))] // Đảm bảo giá trị hợp lệ
        public OrderItemShopStatus NewStatus { get; set; }
    }
    public class OrderHistoryDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int TotalItems { get; set; } // Tổng số loại sản phẩm trong đơn

        // Thông tin tóm tắt của sản phẩm đầu tiên
        public string PrimaryProductName { get; set; } = string.Empty;
        public string? PrimaryProductImage { get; set; }
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
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
}

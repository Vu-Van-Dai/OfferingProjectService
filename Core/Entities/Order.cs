using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    // Trạng thái đơn hàng
    public enum OrderStatus
    {
        Pending, // Chờ xử lý/thanh toán
        Processing, // Đang xử lý
        Shipped, // Đã giao hàng
        Completed, // Hoàn thành
        Cancelled // Đã hủy
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; } // Tổng tiền hàng (chưa gồm ship)

        // Phí vận chuyển, voucher... có thể thêm sau

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } // Tổng tiền cuối cùng

        // Địa chỉ giao hàng được nhúng trực tiếp vào Order
        // ✅ SỬA LỖI #8:
        public ShippingAddress ShippingAddress { get; set; } = null!;

        // Mối quan hệ: Đơn hàng của ai
        public Guid BuyerUserId { get; set; }
        // ✅ SỬA LỖI #8:
        public AppUser Buyer { get; set; } = null!;

        // Mối quan hệ: Đơn hàng gồm những món nào
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
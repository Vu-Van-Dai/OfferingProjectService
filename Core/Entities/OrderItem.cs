using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    // Chi tiết một món hàng trong đơn hàng
    public enum OrderItemShopStatus
    {
        Pending,        // Chờ Shop xác nhận
        Preparing,      // Shop đang chuẩn bị
        ReadyToShip,    // Sẵn sàng giao
        Shipped,        // Đã giao cho vận chuyển
        Cancelled       // Shop đã hủy
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Giá sản phẩm tại thời điểm mua

        // Mối quan hệ: Thuộc đơn hàng nào
        public int OrderId { get; set; }
        public Order Order { get; set; }

        // Mối quan hệ: Là sản phẩm nào
        public int ProductId { get; set; }
        public Product ProductOrdered { get; set; } // Đổi tên để tránh trùng

        // Mối quan hệ: Thuộc cửa hàng nào (để xử lý đơn cho shop)
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public OrderItemShopStatus ShopStatus { get; set; } = OrderItemShopStatus.Pending;
    }
}

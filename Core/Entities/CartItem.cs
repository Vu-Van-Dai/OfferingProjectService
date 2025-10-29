using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    // Một món hàng trong giỏ
    public class CartItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public bool IsSelected { get; set; } = true; // Để chọn món hàng thanh toán

        // Thuộc về giỏ hàng nào
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        // Là sản phẩm nào
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Thuộc cửa hàng nào (để nhóm)
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    // Giỏ hàng tổng của một người dùng
    public class Cart
    {
        public int Id { get; set; }

        // Mối quan hệ 1-1 với người dùng (Bắt buộc)
        public Guid OwnerUserId { get; set; }
        public AppUser OwnerUser { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}

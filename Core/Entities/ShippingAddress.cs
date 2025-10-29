using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    // Sử dụng owned entity type, nên không cần Id riêng
    // Địa chỉ giao hàng
    public class ShippingAddress
    {
        [Required]
        public string FullName { get; set; } // Tên người nhận

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Street { get; set; } // Số nhà, tên đường

        [Required]
        public string Ward { get; set; } // Phường/Xã

        [Required]
        public string District { get; set; } // Quận/Huyện

        [Required]
        public string City { get; set; } // Tỉnh/Thành phố
    }
}

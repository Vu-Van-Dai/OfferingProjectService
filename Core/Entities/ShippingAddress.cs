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
        // ✅ SỬA LỖI #5:
        [Required]
        public string FullName { get; set; } = string.Empty; // Tên người nhận

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Street { get; set; } = string.Empty; // Số nhà, tên đường

        [Required]
        public string Ward { get; set; } = string.Empty; // Phường/Xã

        [Required]
        public string District { get; set; } = string.Empty; // Quận/Huyện

        [Required]
        public string City { get; set; } = string.Empty; // Tỉnh/Thành phố
    }
}
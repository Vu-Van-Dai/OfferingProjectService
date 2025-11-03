using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class UserAddress
    {
        public int Id { get; set; }

        // ✅ SỬA LỖI #6:
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

        public bool IsDefault { get; set; } = false; // Đánh dấu địa chỉ mặc định

        // Mối quan hệ: Địa chỉ này của ai
        public Guid UserId { get; set; }
        // ✅ SỬA LỖI #8:
        public AppUser User { get; set; } = null!;
    }
}
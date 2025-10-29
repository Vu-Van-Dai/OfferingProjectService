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

        public bool IsDefault { get; set; } = false; // Đánh dấu địa chỉ mặc định

        // Mối quan hệ: Địa chỉ này của ai
        public Guid UserId { get; set; }
        public AppUser User { get; set; }
    }
}

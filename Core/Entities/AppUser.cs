using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class AppUser
    {
        public Guid Id { get; set; } // Dùng Guid làm khóa chính để tránh bị đoán

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? Introduction { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Sẽ lưu mật khẩu đã được băm

        // Tự quản lý quyền bằng một danh sách các chuỗi
        public ICollection<string> Roles { get; set; } = new List<string>();
        // Mối quan hệ: Một người dùng có thể viết nhiều đánh giá
        public int? ShopId { get; set; }
        public Shop? Shop { get; set; }
        public Cart? Cart { get; set; }
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();

    }
}

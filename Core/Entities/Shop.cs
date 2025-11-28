using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Shop
    {
        public int Id { get; set; }

        // ✅ SỬA LỖI #3:
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty; // Tên cửa hàng (Xôi Chè Cô Bốn)
        public string? Description { get; set; }
        public byte[]? AvatarData { get; set; }
        [MaxLength(50)]
        public string? AvatarMimeType { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        [MaxLength(20)]
        public string? ContactPhoneNumber { get; set; }

        // Mối quan hệ 1-1: Một Shop được sở hữu bởi một AppUser
        public Guid OwnerUserId { get; set; }

        // ✅ SỬA LỖI #3:
        [Required]
        [MaxLength(200)]
        public string SearchableName { get; set; } = string.Empty;

        // === THÊM MỚI CHO ADMIN ===
        public bool IsLocked { get; set; } = false; // Admin khóa/mở shop

        [Column(TypeName = "decimal(5,2)")] // Ví dụ: 10.00 (cho 10.00%)
        public decimal? CommissionRate { get; set; } // Hoa hồng riêng, null thì dùng mặc định

        // ✅ SỬA LỖI #8 (và #3):
        public AppUser OwnerUser { get; set; } = null!;

        // Mối quan hệ 1-n: Một Shop có nhiều sản phẩm
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
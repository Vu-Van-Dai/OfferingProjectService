using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    // Đại diện cho "Danh Mục Sản Phẩm"
    // Ví dụ: "Hoa Tươi", "Hương Nến", "Gói Chụp Ảnh"
    public class ProductCategory
    {
        public int Id { get; set; }

        // ✅ SỬA LỖI #4:
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? BannerTitle { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; } // Ảnh icon của danh mục

        // Mối quan hệ: Một danh mục có nhiều sản phẩm
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public bool IsHidden { get; set; } = false; // Admin ẩn/hiện
        public int DisplayOrder { get; set; } = 0; // Admin sắp xếp
    }
}
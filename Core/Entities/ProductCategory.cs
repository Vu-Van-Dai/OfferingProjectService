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

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }
        public string? ImageUrl { get; set; } // Ảnh icon của danh mục

        // Mối quan hệ: Một danh mục có nhiều sản phẩm
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

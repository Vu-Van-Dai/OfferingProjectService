using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    // Đại diện cho một sản phẩm cụ thể
    // Ví dụ: "Hoa Hồng Đỏ", "Nến Thơm Trầm Hương"
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; } // Mô tả ngắn (Cam sành ngọt thanh...)
        public string? Features { get; set; } // Các gạch đầu dòng (Giàu vitamin C, ...)
        public string? ImageUrl { get; set; } // Ảnh chính của sản phẩm
        public bool IsPopular { get; set; } // Để hiển thị tag "Phổ biến"

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; } // Giá gốc (ví dụ: 80.000đ)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxPrice { get; set; } // Giá tối đa (ví dụ: 150.000đ)
                                               // Nếu null, FE sẽ chỉ hiển thị BasePrice
        public int StockQuantity { get; set; } // Số lượng trong kho
        public string? Specifications { get; set; } // "Thông số kỹ thuật" (Lưu dạng JSON hoặc chuỗi)
        // Mối quan hệ: Một sản phẩm thuộc về một danh mục
        public int ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class ProductReview
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } // Số sao (1-5)

        [MaxLength(1000)]
        public string? Comment { get; set; } // Nội dung bình luận
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        // Thông tin người đánh giá
        public string UserName { get; set; } // Tên người dùng (Nguyễn Thu Hương)

        // Mối quan hệ: Một đánh giá thuộc về một sản phẩm
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Mối quan hệ: Một đánh giá có thể được viết bởi 1 người dùng
        // (Có thể null nếu cho phép đánh giá không cần đăng nhập)
        public Guid? UserId { get; set; }
        public AppUser? User { get; set; }
        // Dùng để lưu "Thông số kỹ thuật" (Xuất xứ, Trọng lượng...)
        // Chúng ta sẽ lưu dưới dạng một chuỗi JSON
        public string? Specifications { get; set; }

        // Mối quan hệ: Một sản phẩm có nhiều đánh giá
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}

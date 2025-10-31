using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CreateReviewDto
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
    public class ShopReviewDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        // Thêm trạng thái Phản hồi nếu cần
    }
}

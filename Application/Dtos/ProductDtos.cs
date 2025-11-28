using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public bool IsPopular { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int StockQuantity { get; set; }
        public Dictionary<string, string>? Specifications { get; set; }
        public int ProductCategoryId { get; set; } // ID của danh mục cha
        public List<IFormFile>? ImageFiles { get; set; }

        // 2. Nhận List String (chứa URL hoặc Base64) [THÊM MỚI]
        public List<string>? ImageUrls { get; set; }
    }
    public class UpdateProductDto : CreateProductDto
    {
        // Danh sách ID ảnh cũ muốn giữ lại (nếu FE gửi list rỗng -> xóa hết ảnh cũ)
        public List<int>? KeepImageIds { get; set; }
    }
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Features { get; set; }
        public List<ImageDto> Images { get; set; } = new List<ImageDto>();
        public bool IsPopular { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int StockQuantity { get; set; }
        public Dictionary<string, string>? Specifications { get; set; }

        // Thông tin liên kết (chỉ ID và Tên)
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;

        // Có thể thêm thông tin đánh giá nếu cần (ví dụ: Rating trung bình, số lượng review)
        // public double AverageRating { get; set; }
        // public int ReviewCount { get; set; }
    }
    public class ImageDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty; // Base64 string
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CategorySummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int ProductCount { get; set; } // Số lượng sản phẩm trong danh mục
    }
    public class CategoryDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? BannerTitle { get; set; } // Tiêu đề Banner
        public string? Description { get; set; } // Mô tả dưới Banner
        // ImageUrl ở đây là icon, vì bạn nói banner ảnh sẽ hard code
        public string? ImageUrl { get; set; }
    }
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string? BannerTitle { get; set; } // Thêm ô nhập Tiêu đề Banner
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; } // Upload file Icon
        public string? ImageUrl { get; set; } // Hoặc dùng URL Icon
    }
    public class UpdateCategoryDto
    {
        public string Name { get; set; }
        public string? BannerTitle { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; } // Upload file Icon mới
        public string? ImageUrl { get; set; } // Hoặc dùng URL Icon mới

        public string? ExistingImageUrl { get; set; } // Giữ lại URL icon cũ
    }
}

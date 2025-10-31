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
        public string? Description { get; set; } // Thêm mô tả nếu FE cần
        public string? ImageUrl { get; set; }
        public int ProductCount { get; set; } // Số lượng sản phẩm trong danh mục
    }
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class UpdateCategoryDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}

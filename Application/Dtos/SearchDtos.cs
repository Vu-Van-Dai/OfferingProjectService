using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    // DTO cho một sản phẩm trong kết quả tìm kiếm
    public class ProductSearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public decimal BasePrice { get; set; }
        public string ShopName { get; set; } // Tên cửa hàng bán sản phẩm này
    }

    // DTO cho một cửa hàng trong kết quả tìm kiếm
    public class ShopSearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        // Yêu cầu của bạn: "các sản phẩm nổi bật của shop sẽ hiện ra"
        public List<ProductSearchResultDto> PopularProducts { get; set; } = new List<ProductSearchResultDto>();
    }

    // DTO tổng hợp cho kết quả cuối cùng
    public class GlobalSearchResponseDto
    {
        public List<ShopSearchResultDto> Shops { get; set; } = new List<ShopSearchResultDto>();
        public List<ProductSearchResultDto> Products { get; set; } = new List<ProductSearchResultDto>();
    }
}

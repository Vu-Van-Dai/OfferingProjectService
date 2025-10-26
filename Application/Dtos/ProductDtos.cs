using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPopular { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Specifications { get; set; }
        public int ProductCategoryId { get; set; } // ID của danh mục cha
    }
    public class UpdateProductDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPopular { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Specifications { get; set; }
        public int ProductCategoryId { get; set; }
    }
}

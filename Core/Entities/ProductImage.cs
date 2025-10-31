using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        // Mối quan hệ: Ảnh này thuộc về sản phẩm nào
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}

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

        public byte[] ImageData { get; set; } = Array.Empty<byte>(); // Lưu ảnh

        [MaxLength(50)]
        public string ImageMimeType { get; set; } = "image/jpeg"; // Lưu loại ảnh (jpg, png)
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}

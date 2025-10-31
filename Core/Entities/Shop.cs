using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Shop
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } // Tên cửa hàng (Xôi Chè Cô Bốn)
        public string? Description { get; set; }
        public string? ImageUrl { get; set; } // Avatar của shop
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        [MaxLength(20)]
        public string? ContactPhoneNumber { get; set; }

        // Mối quan hệ 1-1: Một Shop được sở hữu bởi một AppUser
        public Guid OwnerUserId { get; set; }
        [MaxLength(200)]
        public string SearchableName { get; set; }
        public AppUser OwnerUser { get; set; }

        // Mối quan hệ 1-n: Một Shop có nhiều sản phẩm
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

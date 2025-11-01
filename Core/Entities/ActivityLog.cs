using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    // Model cho Nhật ký Hoạt động
    public class ActivityLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Actor { get; set; } = string.Empty; // "Admin", "Shop"

        [MaxLength(200)]
        public string Action { get; set; } = string.Empty; // "Khóa Shop", "Tạo Sản Phẩm"

        public string? Details { get; set; } // "Admin đã khóa Shop 'Xôi Chè Cô Bốn'"
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class AppSetting
    {
        [Key] // Dùng Key làm Khóa chính
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty; // Ví dụ: "DefaultCommissionRate"

        public string Value { get; set; } = string.Empty; // Ví dụ: "10.0"
    }
}

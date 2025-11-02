using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    // === 1. Category Management ===

    // DTO trả về cho Admin xem danh sách Category
    public class AdminCategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public int DisplayOrder { get; set; }
        public int ProductCount { get; set; }
    }

    // DTO nhận vào khi Admin bật/tắt Category
    public class ToggleVisibilityDto
    {
        public bool IsHidden { get; set; }
    }

    // DTO nhận vào khi Admin sắp xếp
    public class ReorderCategoriesDto
    {
        [Required]
        public List<int> OrderedCategoryIds { get; set; } = new List<int>();
    }

    // === 6. Activity Log ===

    // DTO trả về cho Admin xem Log
    public class ActivityLogDto
    {
        public DateTime Timestamp { get; set; }
        public string Actor { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    public class AdminShopListResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string? ContactPhoneNumber { get; set; }
        public bool IsLocked { get; set; }
        public decimal? CommissionRate { get; set; } // Hoa hồng riêng
    }

    // DTO để Admin tạo tài khoản Shop mới
    public class AdminCreateShopDto
    {
        [Required]
        public string ShopName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; } = string.Empty;
    }

    // DTO để Admin chuyển đổi Guest thành Shop
    public class AdminConvertGuestDto
    {
        [Required]
        public string UserEmail { get; set; } = string.Empty; // Dùng Email cho dễ tìm
        [Required]
        public string ShopName { get; set; } = string.Empty;
    }

    // DTO để Admin sửa thông tin cơ bản của Shop
    public class AdminUpdateShopDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? ContactPhoneNumber { get; set; }
    }

    // DTO để Admin khóa/mở Shop
    public class ToggleShopLockDto
    {
        public bool IsLocked { get; set; }
    }

    // DTO để Admin reset mật khẩu Shop
    public class AdminResetPasswordDto
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
    // === 3. DTOs Quản lý Sản phẩm ===

    // DTO trả về danh sách SP cho Admin xem
    public class AdminProductListResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsHidden { get; set; } // Trạng thái
        public string Status { get; set; } = "Hết hàng";
    }

    // DTO nhận vào khi Admin chuyển Category
    public class AdminChangeProductCategoryDto
    {
        public int NewCategoryId { get; set; }
    }
    // === 4. DTOs Quản lý Cấu Hình (Hoa hồng) ===

    // DTO trả về cho Admin xem
    public class CommissionSettingsDto
    {
        public decimal DefaultCommissionRate { get; set; }
        public List<ShopCommissionDto> ShopSpecificRates { get; set; } = new List<ShopCommissionDto>();
    }

    // DTO con cho CommissionSettingsDto
    public class ShopCommissionDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }
    }

    // DTO nhận vào khi Admin cập nhật
    public class UpdateCommissionRateDto
    {
        [Range(0, 100)]
        public decimal Rate { get; set; }
    }
    // === 5. DTOs Dashboard Doanh thu ===

    // DTO cho các số liệu tổng
    public class AdminDashboardStatsDto
    {
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueThisYear { get; set; }
    }

    // DTO cho doanh thu của từng shop
    public class AdminShopRevenueDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; } // Tổng doanh thu (đã hoàn thành)
    }
}

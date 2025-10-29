using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CartDtos
    {
        public class AddItemToCartDto
        {
            [Required]
            public int ProductId { get; set; }

            [Required]
            [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100.")] // Giới hạn số lượng hợp lý
            public int Quantity { get; set; }
        }

        /// <summary>
        /// DTO dùng khi Frontend gửi yêu cầu cập nhật số lượng của một món hàng.
        /// </summary>
        public class UpdateCartItemDto
        {
            [Required]
            [Range(0, 100, ErrorMessage = "Số lượng phải từ 0 đến 100.")] // Cho phép số lượng = 0 (để xóa)
            public int Quantity { get; set; }
        }

        /// <summary>
        /// DTO dùng khi Frontend gửi yêu cầu chọn hoặc bỏ chọn một món hàng để thanh toán.
        /// </summary>
        public class SelectCartItemDto
        {
            [Required]
            public bool IsSelected { get; set; }
        }

        //-----------------------------------------------------
        // Các DTO dùng để Backend trả về thông tin giỏ hàng
        //-----------------------------------------------------

        /// <summary>
        /// DTO chứa thông tin chi tiết của một món hàng trong giỏ trả về cho Frontend.
        /// </summary>
        public class CartItemDto
        {
            public int Id { get; set; } // Id của CartItem (dùng để xóa/sửa)
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty; // Khởi tạo giá trị mặc định
            public string? ImageUrl { get; set; }
            public decimal Price { get; set; } // Giá tại thời điểm lấy giỏ hàng
            public int Quantity { get; set; }
            public bool IsSelected { get; set; } // Trạng thái chọn/bỏ chọn
        }

        /// <summary>
        /// DTO chứa thông tin của một cửa hàng và các món hàng thuộc cửa hàng đó trong giỏ.
        /// </summary>
        public class ShopInCartDto
        {
            public int ShopId { get; set; }
            public string ShopName { get; set; } = string.Empty; // Khởi tạo giá trị mặc định
            public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        }

        /// <summary>
        /// DTO tổng thể cho giỏ hàng trả về cho Frontend, đã nhóm theo cửa hàng.
        /// </summary>
        public class CartResponseDto
        {
            public int Id { get; set; } // Id của Cart
            public List<ShopInCartDto> Shops { get; set; } = new List<ShopInCartDto>();
            public decimal TotalPrice { get; set; } // Tổng tiền của các món hàng ĐÃ CHỌN (IsSelected = true)
        }
    }
}

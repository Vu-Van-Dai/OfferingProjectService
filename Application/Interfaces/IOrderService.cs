using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        // Có thể trả về null nếu địa chỉ không hợp lệ
        Task<OrderResponseDto?> CreateOrderFromCartAsync(Guid userId, CreateOrderRequestDto orderRequest);
        Task<OrderResponseDto?> GetOrderDetailsAsync(Guid userId, int orderId);

        /// <summary>
        /// Lấy lịch sử (tóm tắt) các đơn hàng của người dùng.
        /// </summary>
        Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(Guid userId);
    }
}

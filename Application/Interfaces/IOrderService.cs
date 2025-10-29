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
    }
}

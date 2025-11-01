using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAdminShopService
    {
        Task<IEnumerable<AdminShopListResponseDto>> GetAllShopsAsync();
        Task<AdminShopListResponseDto> CreateShopAccountAsync(AdminCreateShopDto dto);
        Task<AdminShopListResponseDto> ConvertGuestToShopAsync(AdminConvertGuestDto dto);
        Task<bool> UpdateShopInfoAsync(int shopId, AdminUpdateShopDto dto);
        Task<bool> ToggleShopLockAsync(int shopId, bool isLocked);
        Task<bool> ResetShopPasswordAsync(int shopId, string newPassword);
    }
}

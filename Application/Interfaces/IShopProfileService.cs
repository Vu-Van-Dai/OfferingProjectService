using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IShopProfileService
    {
        Task<ShopProfileResponseDto?> GetShopProfileByUserIdAsync(Guid userId);
        Task<bool> UpdateShopProfileAsync(Guid userId, UpdateShopProfileDto dto);
    }
}

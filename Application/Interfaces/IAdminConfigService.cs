using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAdminConfigService
    {
        Task<CommissionSettingsDto> GetCommissionSettingsAsync();
        Task<bool> SetDefaultCommissionAsync(decimal rate);
        Task<bool> SetShopCommissionAsync(int shopId, decimal rate);
    }
}

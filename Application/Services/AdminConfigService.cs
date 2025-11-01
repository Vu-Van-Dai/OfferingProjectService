using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AdminConfigService : IAdminConfigService
    {
        private readonly IAppSettingRepository _settingRepo;
        private readonly IShopRepository _shopRepo;
        private readonly IActivityLogService _logService;

        private const string DefaultCommissionKey = "DefaultCommissionRate";

        public AdminConfigService(IAppSettingRepository settingRepo, IShopRepository shopRepo, IActivityLogService logService)
        {
            _settingRepo = settingRepo;
            _shopRepo = shopRepo;
            _logService = logService;
        }

        public async Task<CommissionSettingsDto> GetCommissionSettingsAsync()
        {
            // 1. Lấy % mặc định
            var defaultRateSetting = await _settingRepo.GetByKeyAsync(DefaultCommissionKey);
            decimal defaultRate = (defaultRateSetting != null && decimal.TryParse(defaultRateSetting.Value, out var rate)) ? rate : 10.0m; // Mặc định 10%

            // 2. Lấy % riêng của các shop
            var shops = await _shopRepo.GetAllWithOwnersAsync(); // Dùng hàm đã tạo ở Phần 2

            var shopRates = shops.Select(s => new ShopCommissionDto
            {
                ShopId = s.Id,
                ShopName = s.Name,
                // Dùng % riêng của Shop, nếu null thì dùng % mặc định
                CommissionRate = s.CommissionRate ?? defaultRate
            }).ToList();

            return new CommissionSettingsDto
            {
                DefaultCommissionRate = defaultRate,
                ShopSpecificRates = shopRates
            };
        }

        public async Task<bool> SetDefaultCommissionAsync(decimal rate)
        {
            var setting = new AppSetting
            {
                Key = DefaultCommissionKey,
                Value = rate.ToString("F2") // Lưu dạng 10.00
            };

            _settingRepo.Upsert(setting);
            await _settingRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Cập nhật Config", $"Đã đặt hoa hồng mặc định thành {rate}%");
            return true;
        }

        public async Task<bool> SetShopCommissionAsync(int shopId, decimal rate)
        {
            var shop = await _shopRepo.GetByIdAsync(shopId);
            if (shop == null) return false;

            shop.CommissionRate = rate;
            _shopRepo.Update(shop);
            await _shopRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Cập nhật Config", $"Đã đặt hoa hồng riêng cho Shop '{shop.Name}' thành {rate}%");
            return true;
        }
    }
}

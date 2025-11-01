﻿using Application.Dtos;
using Application.Interfaces;
using Application.Utils;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AdminShopService : IAdminShopService
    {
        private readonly IShopRepository _shopRepo;
        private readonly IUserRepository _userRepo;
        private readonly IActivityLogService _logService; // Inject dịch vụ Log

        public AdminShopService(IShopRepository shopRepo, IUserRepository userRepo, IActivityLogService logService)
        {
            _shopRepo = shopRepo;
            _userRepo = userRepo;
            _logService = logService;
        }

        public async Task<IEnumerable<AdminShopListResponseDto>> GetAllShopsAsync()
        {
            var shops = await _shopRepo.GetAllWithOwnersAsync();
            // Map sang DTO
            return shops.Select(s => new AdminShopListResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                OwnerEmail = s.OwnerUser?.Email ?? "N/A",
                ContactPhoneNumber = s.ContactPhoneNumber,
                IsLocked = s.IsLocked,
                CommissionRate = s.CommissionRate
            });
        }

        public async Task<AdminShopListResponseDto> CreateShopAccountAsync(AdminCreateShopDto dto)
        {
            // 1. Kiểm tra User Email đã tồn tại chưa
            if (await _userRepo.GetByEmailAsync(dto.Email) != null)
            {
                throw new Exception("Email đã tồn tại.");
            }

            // 2. Tạo AppUser mới
            var newUser = new AppUser
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Roles = new List<string> { "Shop" } // Gán quyền "Shop" ngay
            };

            // 3. Tạo Shop mới
            var newShop = new Shop
            {
                Name = dto.ShopName,
                SearchableName = StringUtils.RemoveAccents(dto.ShopName),
                OwnerUser = newUser // Gán User vào Shop
            };

            // 4. Liên kết User với Shop (quan trọng)
            newUser.Shop = newShop;

            // 5. Lưu vào DB (EF Core sẽ tự xử lý cả 2)
            await _userRepo.AddAsync(newUser); // Add User (vì User là gốc)
            await _userRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Tạo Shop", $"Đã tạo Shop '{dto.ShopName}' với email '{dto.Email}'");

            return (await GetAllShopsAsync()).First(s => s.Id == newShop.Id);
        }

        public async Task<AdminShopListResponseDto> ConvertGuestToShopAsync(AdminConvertGuestDto dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.UserEmail);
            if (user == null) throw new Exception("Không tìm thấy người dùng với email này.");
            if (user.ShopId != null) throw new Exception("Người dùng này đã sở hữu một cửa hàng.");

            // Tạo Shop mới
            var newShop = new Shop
            {
                Name = dto.ShopName,
                SearchableName = StringUtils.RemoveAccents(dto.ShopName),
                OwnerUserId = user.Id
            };
            await _shopRepo.AddAsync(newShop);
            await _shopRepo.SaveChangesAsync(); // Lưu để lấy ShopId

            // Cập nhật User
            user.ShopId = newShop.Id;
            if (!user.Roles.Contains("Shop"))
            {
                user.Roles.Add("Shop"); // Thêm quyền "Shop"
            }
            await _userRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Chuyển đổi tài khoản", $"Đã chuyển Guest '{dto.UserEmail}' thành Shop '{dto.ShopName}'");

            return (await GetAllShopsAsync()).First(s => s.Id == newShop.Id);
        }

        public async Task<bool> UpdateShopInfoAsync(int shopId, AdminUpdateShopDto dto)
        {
            var shop = await _shopRepo.GetByIdAsync(shopId);
            if (shop == null) return false;

            shop.Name = dto.Name;
            shop.SearchableName = StringUtils.RemoveAccents(dto.Name);
            shop.ContactPhoneNumber = dto.ContactPhoneNumber;

            _shopRepo.Update(shop);
            await _shopRepo.SaveChangesAsync();

            await _logService.LogAsync("Admin", "Sửa Shop", $"Đã sửa thông tin Shop ID: {shopId}");
            return true;
        }

        public async Task<bool> ToggleShopLockAsync(int shopId, bool isLocked)
        {
            var shop = await _shopRepo.GetByIdAsync(shopId);
            if (shop == null) return false;

            shop.IsLocked = isLocked;
            _shopRepo.Update(shop);
            await _shopRepo.SaveChangesAsync();

            var action = isLocked ? "Khóa" : "Mở khóa";
            await _logService.LogAsync("Admin", "Khóa/Mở Shop", $"{action} Shop: {shop.Name} (ID: {shopId})");
            return true;
        }

        public async Task<bool> ResetShopPasswordAsync(int shopId, string newPassword)
        {
            var shop = await _shopRepo.GetByIdWithOwnerAsync(shopId); // Cần thông tin OwnerUser
            if (shop == null || shop.OwnerUser == null) return false;

            var user = shop.OwnerUser;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Không cần gọi _userRepo.Update() vì EF Core đang theo dõi đối tượng
            await _shopRepo.SaveChangesAsync(); // Lưu thông qua _shopRepo (cùng DbContext)

            await _logService.LogAsync("Admin", "Reset Mật khẩu", $"Đã reset mật khẩu cho Shop: {shop.Name}");
            return true;
        }
    }
}

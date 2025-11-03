using Application.Dtos;
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
    public class ShopProfileService : IShopProfileService
    {
        private readonly IShopRepository _shopRepository;
        private readonly IImageService _imageService;

        public ShopProfileService(IShopRepository shopRepository, IImageService imageService)
        {
            _shopRepository = shopRepository;
            _imageService = imageService;
        }

        public async Task<ShopProfileResponseDto?> GetShopProfileByUserIdAsync(Guid userId)
        {
            var shop = await _shopRepository.GetByOwnerUserIdAsync(userId);
            if (shop == null) return null;
            return MapToDto(shop);
        }

        public async Task<bool> UpdateShopProfileAsync(Guid userId, UpdateShopProfileDto dto)
        {
            var shop = await _shopRepository.GetByOwnerUserIdAsync(userId);
            if (shop == null) return false;

            string? newImageUrl = shop.ImageUrl; // Giữ ảnh cũ mặc định
            string? oldImageUrl = shop.ImageUrl; // Lưu đường dẫn ảnh cũ để xóa sau

            // Kiểm tra file upload hợp lệ
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                // Lưu ảnh mới TRƯỚC
                var savedImageUrl = await _imageService.SaveImageAsync(dto.ImageFile, "shops");
                
                // Chỉ cập nhật và xóa ảnh cũ nếu lưu ảnh mới thành công
                if (!string.IsNullOrEmpty(savedImageUrl))
                {
                    newImageUrl = savedImageUrl;
                    // Xóa ảnh cũ nếu nó tồn tại
                    _imageService.DeleteImage(oldImageUrl);
                }
                // Nếu SaveImageAsync trả về null, giữ nguyên ảnh cũ
            }
            // Không có else if cho dto.ImageUrl vì chúng ta ưu tiên file

            // Cập nhật thông tin shop
            shop.Name = dto.Name;
            shop.Description = dto.Description;
            shop.ContactPhoneNumber = dto.ContactPhoneNumber;
            shop.ImageUrl = newImageUrl; // Cập nhật link ảnh mới (hoặc giữ link cũ)
            shop.SearchableName = StringUtils.RemoveAccents(dto.Name); // Cập nhật tên tìm kiếm

            _shopRepository.Update(shop);
            await _shopRepository.SaveChangesAsync();
            return true;
        }

        private ShopProfileResponseDto MapToDto(Shop shop) => new ShopProfileResponseDto
        {
            Id = shop.Id,
            Name = shop.Name,
            Description = shop.Description,
            ImageUrl = shop.ImageUrl,
            ContactPhoneNumber = shop.ContactPhoneNumber,
            JoinDate = shop.JoinDate
        };
    }
}

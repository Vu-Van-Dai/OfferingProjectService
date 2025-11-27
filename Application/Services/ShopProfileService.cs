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

            return new ShopProfileResponseDto
            {
                Id = shop.Id,
                Name = shop.Name,
                Description = shop.Description,
                ContactPhoneNumber = shop.ContactPhoneNumber,
                JoinDate = shop.JoinDate,
                AvatarBase64 = _imageService.ToBase64(shop.AvatarData, shop.AvatarMimeType)
            };
        }

        public async Task<bool> UpdateShopProfileAsync(Guid userId, UpdateShopProfileDto dto)
        {
            var shop = await _shopRepository.GetByOwnerUserIdAsync(userId);
            if (shop == null) return false;

            shop.Name = dto.Name;
            shop.Description = dto.Description;
            shop.ContactPhoneNumber = dto.ContactPhoneNumber;
            shop.SearchableName = StringUtils.RemoveAccents(dto.Name);

            byte[]? newData = null;
            string? newMime = null;

            if (dto.AvatarFile != null)
            {
                var result = await _imageService.ProcessImageAsync(dto.AvatarFile);
                newData = result.Data;
                newMime = result.MimeType;
            }
            else if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                var result = await _imageService.ProcessStringImageAsync(dto.AvatarUrl);
                if (result != null)
                {
                    newData = result.Value.Data;
                    newMime = result.Value.MimeType;
                }
            }

            if (newData != null)
            {
                shop.AvatarData = newData;
                shop.AvatarMimeType = newMime;
            }

            _shopRepository.Update(shop);
            await _shopRepository.SaveChangesAsync();
            return true;
        }
    }
}

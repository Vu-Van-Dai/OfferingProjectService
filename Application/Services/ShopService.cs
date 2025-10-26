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
    public class ShopService : IShopService
    {
        private readonly IShopRepository _shopRepository;
        private readonly IUserRepository _userRepository;

        public ShopService(IShopRepository shopRepository, IUserRepository userRepository)
        {
            _shopRepository = shopRepository;
            _userRepository = userRepository;
        }

        public async Task<Shop> CreateShopAsync(Guid ownerUserId, CreateShopDto shopDto)
        {
            var user = await _userRepository.GetByIdAsync(ownerUserId);
            if (user == null) throw new Exception("Không tìm thấy người dùng.");
            if (user.ShopId != null) throw new Exception("Người dùng này đã sở hữu một cửa hàng.");

            var newShop = new Shop
            {
                Name = shopDto.ShopName,
                Description = shopDto.Description,
                SearchableName = StringUtils.RemoveAccents(shopDto.ShopName),
                OwnerUserId = ownerUserId
            };

            await _shopRepository.AddAsync(newShop);
            await _shopRepository.SaveChangesAsync(); // Lưu để lấy ShopId

            // Cập nhật lại AppUser
            user.ShopId = newShop.Id;
            if (!user.Roles.Contains("Shop"))
            {
                user.Roles.Add("Shop"); // Tự động cấp quyền "Shop"
            }
            await _userRepository.SaveChangesAsync(); // Lưu thay đổi cho user

            return newShop;
        }
    }
}

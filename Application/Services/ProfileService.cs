using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IImageService _imageService;

        public ProfileService(IUserRepository userRepository, IImageService imageService)
        {
            _userRepository = userRepository;
            _imageService = imageService;
        }

        public async Task<AppUser?> GetProfileAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto profileDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            string? finalAvatarUrl = user.AvatarUrl; // Giữ ảnh cũ mặc định

            if (profileDto.AvatarFile != null && profileDto.AvatarFile.Length > 0)
            {
                // Xóa ảnh cũ nếu nó tồn tại (và không phải là link mặc định)
                _imageService.DeleteImage(user.AvatarUrl);

                // Lưu ảnh mới vào thư mục "avatars"
                finalAvatarUrl = await _imageService.SaveImageAsync(profileDto.AvatarFile, "avatars");
            }

            user.FullName = profileDto.FullName;
            user.PhoneNumber = profileDto.PhoneNumber;
            user.Introduction = profileDto.Introduction;
            user.AvatarUrl = finalAvatarUrl;

            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}

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

            try
            {
                string? finalAvatarUrl = user.AvatarUrl; // Giữ ảnh cũ mặc định
                string? oldAvatarUrl = user.AvatarUrl; // Lưu đường dẫn ảnh cũ để xóa sau

                if (profileDto.AvatarFile != null && profileDto.AvatarFile.Length > 0)
                {
                    // Lưu ảnh mới vào thư mục "avatars" TRƯỚC
                    var newAvatarUrl = await _imageService.SaveImageAsync(profileDto.AvatarFile, "avatars");

                    // Chỉ cập nhật và xóa ảnh cũ nếu lưu ảnh mới thành công
                    if (!string.IsNullOrEmpty(newAvatarUrl))
                    {
                        finalAvatarUrl = newAvatarUrl;
                        // Xóa ảnh cũ nếu nó tồn tại (và không phải là link mặc định)
                        _imageService.DeleteImage(oldAvatarUrl);
                    }
                    // Nếu SaveImageAsync trả về null, giữ nguyên ảnh cũ (finalAvatarUrl vẫn = user.AvatarUrl)
                }

                // Cập nhật thông tin user - FullName là required nên luôn có giá trị
                if (string.IsNullOrWhiteSpace(profileDto.FullName))
                {
                    throw new ArgumentException("FullName không được để trống.");
                }
                user.FullName = profileDto.FullName;
                user.PhoneNumber = profileDto.PhoneNumber;
                user.Introduction = profileDto.Introduction;
                user.AvatarUrl = finalAvatarUrl;

                // Lưu vào database
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (ArgumentException)
            {
                // Re-throw ArgumentException để controller có thể xử lý
                throw;
            }
            catch (Exception ex)
            {
                // Log lỗi và throw để controller xử lý
                throw new Exception($"Lỗi khi cập nhật profile: {ex.Message}", ex);
            }

        }
    }
}

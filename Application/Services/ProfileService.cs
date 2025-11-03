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

                if (profileDto.AvatarFile != null && profileDto.AvatarFile.Length > 0)
                {
                    // Convert ảnh thành base64 và lưu vào database (KHÔNG tạo file trên server)
                    var base64Avatar = await _imageService.ConvertToBase64Async(profileDto.AvatarFile);

                    // Chỉ cập nhật nếu convert thành công
                    if (!string.IsNullOrEmpty(base64Avatar))
                    {
                        finalAvatarUrl = base64Avatar;
                    }
                    // Nếu ConvertToBase64Async trả về null, giữ nguyên ảnh cũ
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
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật profile: {ex.Message}", ex);
            }
        }
    }
}

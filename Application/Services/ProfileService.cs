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
            if (user == null) return false;

            user.FullName = profileDto.FullName;
            user.PhoneNumber = profileDto.PhoneNumber;
            user.Introduction = profileDto.Introduction;

            if (profileDto.AvatarFile != null)
            {
                var result = await _imageService.ProcessImageAsync(profileDto.AvatarFile);
                user.AvatarData = result.Data;
                user.AvatarMimeType = result.MimeType;
            }
            // Xử lý Avatar (URL/Base64 -> byte[])
            else if (!string.IsNullOrEmpty(profileDto.AvatarUrl))
            {
                var result = await _imageService.ProcessStringImageAsync(profileDto.AvatarUrl);
                if (result != null)
                {
                    user.AvatarData = result.Value.Data;
                    user.AvatarMimeType = result.Value.MimeType;
                }
            }

            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}

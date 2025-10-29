using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;

        public ProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AppUser?> GetProfileAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber,string? introduction)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.Introduction = introduction;

            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}

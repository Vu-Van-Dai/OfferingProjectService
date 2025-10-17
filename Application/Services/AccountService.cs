using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;

        public AccountService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AppUser?> RegisterAsync(string fullName, string email, string password)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                return null;
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Roles = new List<string> { "Guest" }
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }

        public async Task<AppUser?> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isPasswordValid ? user : null;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // Xác thực mật khẩu cũ
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                return false;
            }

            // Băm và cập nhật mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAccountAsync(Guid userId, string password)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // Yêu cầu xác thực mật khẩu trước khi xóa
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false;
            }

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}

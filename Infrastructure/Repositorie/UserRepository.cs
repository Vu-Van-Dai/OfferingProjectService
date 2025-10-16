using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositorie
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            // Dùng DbContext để tìm người dùng trong database
            return await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(AppUser user)
        {
            // Dùng DbContext để thêm người dùng mới
            await _context.AppUsers.AddAsync(user);
        }

        public async Task<int> SaveChangesAsync()
        {
            // Dùng DbContext để lưu tất cả thay đổi vào database
            return await _context.SaveChangesAsync();
        }
    }
}

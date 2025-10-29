using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositorie
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly ApplicationDbContext _context;
        public UserAddressRepository(ApplicationDbContext context) { _context = context; }

        public async Task<UserAddress?> GetByIdAsync(int id)
        {
            return await _context.UserAddresses.FindAsync(id);
        }

        public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault) // Mặc định lên đầu
                .ToListAsync();
        }

        public async Task<UserAddress?> GetDefaultForUserAsync(Guid userId)
        {
            return await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
        }

        public async Task AddAsync(UserAddress address)
        {
            await _context.UserAddresses.AddAsync(address);
        }

        public void Update(UserAddress address)
        {
            _context.UserAddresses.Update(address);
        }

        public void Delete(UserAddress address)
        {
            _context.UserAddresses.Remove(address);
        }

        public async Task UnsetDefaultForUserAsync(Guid userId)
        {
            // Tìm các địa chỉ mặc định cũ và bỏ đánh dấu
            var oldDefaults = await _context.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();
            foreach (var addr in oldDefaults)
            {
                addr.IsDefault = false;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByEmailAsync(string email);
        Task<AppUser?> GetByIdAsync(Guid id);
        Task AddAsync(AppUser user);
        void Remove(AppUser user);
        Task<int> SaveChangesAsync();
    }
}

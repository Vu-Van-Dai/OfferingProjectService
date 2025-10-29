using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserAddressRepository
    {
        Task<UserAddress?> GetByIdAsync(int id);
        Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId);
        Task<UserAddress?> GetDefaultForUserAsync(Guid userId);
        Task AddAsync(UserAddress address);
        void Update(UserAddress address);
        void Delete(UserAddress address);
        Task UnsetDefaultForUserAsync(Guid userId); // Bỏ mặc định cũ
        Task<int> SaveChangesAsync();
    }
}

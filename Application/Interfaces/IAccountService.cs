using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<AppUser?> RegisterAsync(string fullName, string email, string password);
        Task<AppUser?> ValidateCredentialsAsync(string email, string password);
        Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<bool> DeleteAccountAsync(Guid userId, string password);
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProfileService
    {
        Task<AppUser?> GetProfileAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber, string? introduction);
    }
}

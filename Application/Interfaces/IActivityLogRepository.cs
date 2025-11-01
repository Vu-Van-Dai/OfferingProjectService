using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IActivityLogRepository
    {
        Task AddAsync(ActivityLog log);
        Task<IEnumerable<ActivityLog>> GetLatestAsync(int count);
        Task<int> SaveChangesAsync();
    }
}

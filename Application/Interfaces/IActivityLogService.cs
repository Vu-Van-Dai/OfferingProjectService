using System;
using Application.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IActivityLogService
    {
        // Hàm này được các service khác gọi
        Task LogAsync(string actor, string action, string? details);

        // Hàm này cho AdminController gọi
        Task<IEnumerable<ActivityLogDto>> GetLatestLogsAsync(int count);
    }
}

using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Repositorie
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly ApplicationDbContext _context;
        public ActivityLogRepository(ApplicationDbContext context) { _context = context; }

        public async Task AddAsync(ActivityLog log)
        {
            await _context.ActivityLogs.AddAsync(log);
        }

        public async Task<IEnumerable<ActivityLog>> GetLatestAsync(int count)
        {
            return await _context.ActivityLogs
                .OrderByDescending(log => log.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

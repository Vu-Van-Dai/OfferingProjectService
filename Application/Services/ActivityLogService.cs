using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _logRepository;

        public ActivityLogService(IActivityLogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task LogAsync(string actor, string action, string? details)
        {
            var log = new ActivityLog
            {
                Actor = actor,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            await _logRepository.AddAsync(log);
            await _logRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ActivityLogDto>> GetLatestLogsAsync(int count)
        {
            var logs = await _logRepository.GetLatestAsync(count);
            return logs.Select(log => new ActivityLogDto
            {
                Timestamp = log.Timestamp,
                Actor = log.Actor,
                Action = log.Action,
                Details = log.Details
            });
        }
    }
}

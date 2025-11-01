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
    public class AppSettingRepository : IAppSettingRepository
    {
        private readonly ApplicationDbContext _context;
        public AppSettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppSetting?> GetByKeyAsync(string key)
        {
            return await _context.AppSettings.FindAsync(key);
        }

        public void Upsert(AppSetting setting)
        {
            // Kiểm tra xem entry đã được theo dõi chưa
            var existingLocal = _context.AppSettings.Local.FirstOrDefault(e => e.Key == setting.Key);
            if (existingLocal != null)
            {
                // Nếu đã được theo dõi, cập nhật nó
                _context.Entry(existingLocal).CurrentValues.SetValues(setting);
            }
            else
            {
                // Nếu chưa, kiểm tra trong DB (không theo dõi)
                var existingDb = _context.AppSettings.AsNoTracking().FirstOrDefault(e => e.Key == setting.Key);
                if (existingDb != null)
                {
                    // Nếu có trong DB, Update
                    _context.AppSettings.Update(setting);
                }
                else
                {
                    // Nếu không có, Add
                    _context.AppSettings.Add(setting);
                }
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAppSettingRepository
    {
        Task<AppSetting?> GetByKeyAsync(string key);
        void Upsert(AppSetting setting); // Hàm này vừa Add vừa Update
        Task<int> SaveChangesAsync();
    }
}

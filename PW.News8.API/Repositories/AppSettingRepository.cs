using Microsoft.EntityFrameworkCore;
using PW.News8.API.Data;
using PW.News8.Shared.Interfaces;
using PW.News8.Shared.Models;

namespace PW.News8.API.Repositories
{
    public class AppSettingRepository : IAppSettingRepository
    {
        private readonly ApplicationDbContext _context;

        public AppSettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppSetting>> GetAllAsync()
        {
            return await _context.AppSettings.ToListAsync();
        }

        public async Task<AppSetting?> GetByKeyAsync(string key)
        {
            return await _context.AppSettings
                .FirstOrDefaultAsync(a => a.Key == key);
        }

        public async Task UpsertAsync(AppSetting setting)
        {
            var existing = await _context.AppSettings
                .FirstOrDefaultAsync(a => a.Key == setting.Key);

            if (existing == null)
            {
                await _context.AppSettings.AddAsync(setting);
            }
            else
            {
                existing.Value = setting.Value;
                existing.Description = setting.Description;
                existing.IsSecret = setting.IsSecret;
                existing.UpdatedAt = DateTime.Now;
                _context.AppSettings.Update(existing);
            }

            await _context.SaveChangesAsync();
        }
    }
}
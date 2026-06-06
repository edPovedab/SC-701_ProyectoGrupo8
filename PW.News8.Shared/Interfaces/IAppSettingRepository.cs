using PW.News8.Shared.Models;

namespace PW.News8.Shared.Interfaces
{
    public interface IAppSettingRepository
    {
        Task<IEnumerable<AppSetting>> GetAllAsync();
        Task<AppSetting?> GetByKeyAsync(string key);
        Task UpsertAsync(AppSetting setting); // Insert si no existe, Update si ya existe
    }
}
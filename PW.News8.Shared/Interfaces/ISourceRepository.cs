using PW.News8.Shared.Models;

namespace PW.News8.Shared.Interfaces
{
    public interface ISourceRepository
    {
        Task<IEnumerable<Source>> GetAllAsync();
        Task<Source?> GetByIdAsync(int id);
        Task AddAsync(Source source);
        Task UpdateAsync(Source source);
        Task DeleteAsync(int id);
    }
}
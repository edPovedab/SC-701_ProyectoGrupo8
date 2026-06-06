using PW.News8.Shared.Models;

namespace PW.News8.Shared.Interfaces
{
    public interface ISourceItemRepository
    {
        Task<IEnumerable<SourceItem>> GetAllAsync();
        Task<IEnumerable<SourceItem>> GetBySourceIdAsync(int sourceId);
        Task<SourceItem?> GetByIdAsync(int id);
        Task AddAsync(SourceItem item);
        Task AddRangeAsync(IEnumerable<SourceItem> items);
        Task DeleteAsync(int id);
    }
}

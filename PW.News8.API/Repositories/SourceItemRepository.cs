using Microsoft.EntityFrameworkCore;
using PW.News8.API.Data;
using PW.News8.Shared.Interfaces;
using PW.News8.Shared.Models;

namespace PW.News8.API.Repositories
{
    public class SourceItemRepository : ISourceItemRepository
    {
        private readonly ApplicationDbContext _context;

        public SourceItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SourceItem>> GetAllAsync()
        {
            return await _context.SourceItems
                .Include(si => si.Source)
                .OrderByDescending(si => si.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SourceItem>> GetBySourceIdAsync(int sourceId)
        {
            return await _context.SourceItems
                .Where(si => si.SourceId == sourceId)
                .OrderByDescending(si => si.CreatedAt)
                .ToListAsync();
        }

        public async Task<SourceItem?> GetByIdAsync(int id)
        {
            return await _context.SourceItems
                .Include(si => si.Source)
                .FirstOrDefaultAsync(si => si.Id == id);
        }

        public async Task AddAsync(SourceItem item)
        {
            await _context.SourceItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<SourceItem> items)
        {
            await _context.SourceItems.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.SourceItems.FindAsync(id);
            if (item != null)
            {
                _context.SourceItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}

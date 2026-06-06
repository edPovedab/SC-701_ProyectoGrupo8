using Microsoft.EntityFrameworkCore;
using PW.News8.API.Data;
using PW.News8.Shared.Interfaces;
using PW.News8.Shared.Models;

namespace PW.News8.API.Repositories
{
    public class SourceRepository : ISourceRepository
    {
        private readonly ApplicationDbContext _context;

        public SourceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Source>> GetAllAsync()
        {
            return await _context.Sources.ToListAsync();
        }

        public async Task<Source?> GetByIdAsync(int id)
        {
            return await _context.Sources
                .Include(s => s.SourceItems)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Source source)
        {
            await _context.Sources.AddAsync(source);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Source source)
        {
            _context.Sources.Update(source);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var source = await _context.Sources.FindAsync(id);
            if (source != null)
            {
                _context.Sources.Remove(source);
                await _context.SaveChangesAsync();
            }
        }
    }
}
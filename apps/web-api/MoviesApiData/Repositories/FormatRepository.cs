using Microsoft.EntityFrameworkCore;
using MoviesApiData.Data;
using MoviesApiDomain.Models;

namespace MoviesApiData.Repositories;

public class FormatRepository : IFormatRepository
{
    private readonly MoviesDbContext _context;

    public FormatRepository(MoviesDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Format>> GetAllAsync()
    {
        return await _context.Formats.ToListAsync();
    }

    public async Task<Format?> GetByIdAsync(int id)
    {
        return await _context.Formats.FindAsync(id);
    }

    public async Task<Format> CreateAsync(Format format)
    {
        _context.Formats.Add(format);
        await _context.SaveChangesAsync();
        return format;
    }

    public async Task<Format?> UpdateAsync(int id, Format format)
    {
        var existing = await _context.Formats.FindAsync(id);
        if (existing == null)
            return null;

        existing.Name = format.Name;
        existing.Description = format.Description;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Formats.FindAsync(id);
        if (existing == null)
            return false;

        _context.Formats.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}

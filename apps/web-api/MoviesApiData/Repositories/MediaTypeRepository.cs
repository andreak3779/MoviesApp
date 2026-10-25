using Microsoft.EntityFrameworkCore;
using MoviesApiData.Data;
using MoviesApiDomain.Models;

namespace MoviesApiData.Repositories;

public class MediaTypeRepository : IMediaTypeRepository
{
    private readonly MoviesDbContext _context;

    public MediaTypeRepository(MoviesDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MediaType>> GetAllAsync()
    {
        return await _context.MediaTypes.ToListAsync();
    }

    public async Task<MediaType?> GetByIdAsync(int id)
    {
        return await _context.MediaTypes.FindAsync(id);
    }

    public async Task<MediaType> CreateAsync(MediaType mediaType)
    {
        _context.MediaTypes.Add(mediaType);
        await _context.SaveChangesAsync();
        return mediaType;
    }

    public async Task<MediaType?> UpdateAsync(int id, MediaType mediaType)
    {
        var existing = await _context.MediaTypes.FindAsync(id);
        if (existing == null)
            return null;

        existing.Name = mediaType.Name;
        existing.Description = mediaType.Description;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.MediaTypes.FindAsync(id);
        if (existing == null)
            return false;

        _context.MediaTypes.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;
using MoviesApi.Data;
using MoviesApi.Models;

namespace MoviesApi.Repositories;

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
}

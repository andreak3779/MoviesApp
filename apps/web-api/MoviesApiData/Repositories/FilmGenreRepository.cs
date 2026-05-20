using Microsoft.EntityFrameworkCore;
using MoviesApiData.Data;
using MoviesApiDomain.Models;

namespace MoviesApiData.Repositories;

public class FilmGenreRepository : IFilmGenreRepository
{
    private readonly MoviesDbContext _context;

    public FilmGenreRepository(MoviesDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FilmGenre>> GetAllAsync()
    {
        return await _context.FilmGenres.ToListAsync();
    }

    public async Task<FilmGenre?> GetByIdAsync(int id)
    {
        return await _context.FilmGenres.FindAsync(id);
    }
}

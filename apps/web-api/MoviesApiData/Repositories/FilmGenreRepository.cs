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

    public async Task<FilmGenre> CreateAsync(FilmGenre filmGenre)
    {
        _context.FilmGenres.Add(filmGenre);
        await _context.SaveChangesAsync();
        return filmGenre;
    }

    public async Task<FilmGenre?> UpdateAsync(int id, FilmGenre filmGenre)
    {
        var existing = await _context.FilmGenres.FindAsync(id);
        if (existing == null)
            return null;

        existing.Name = filmGenre.Name;
        existing.Description = filmGenre.Description;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.FilmGenres.FindAsync(id);
        if (existing == null)
            return false;

        _context.FilmGenres.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}

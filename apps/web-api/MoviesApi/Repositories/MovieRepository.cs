using Microsoft.EntityFrameworkCore;
using MoviesApi.Data;
using MoviesApi.Models;

namespace MoviesApi.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MoviesDbContext _context;

    public MovieRepository(MoviesDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        return await _context.Movies
            .Include(m => m.MovieMediaTypes)
                .ThenInclude(mm => mm.MediaType)
            .Include(m => m.MovieFilmGenres)
                .ThenInclude(mf => mf.FilmGenre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Movie>> GetAllByUserAsync(string userId)
    {
        return await _context.Movies
            .Where(m => m.OwnerId == userId)
            .Include(m => m.MovieMediaTypes)
                .ThenInclude(mm => mm.MediaType)
            .Include(m => m.MovieFilmGenres)
                .ThenInclude(mf => mf.FilmGenre)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Movie> Movies, int TotalCount)> GetPagedByUserAsync(string userId, int pageNumber, int pageSize)
    {
        var query = _context.Movies
            .Where(m => m.OwnerId == userId)
            .Include(m => m.MovieMediaTypes)
                .ThenInclude(mm => mm.MediaType)
            .Include(m => m.MovieFilmGenres)
                .ThenInclude(mf => mf.FilmGenre)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var movies = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (movies, totalCount);
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        return await _context.Movies
            .Include(m => m.MovieMediaTypes)
                .ThenInclude(mm => mm.MediaType)
            .Include(m => m.MovieFilmGenres)
                .ThenInclude(mf => mf.FilmGenre)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Movie?> GetByIdWithOwnershipCheckAsync(Guid id, string userId)
    {
        var movie = await _context.Movies
            .Include(m => m.MovieMediaTypes)
                .ThenInclude(mm => mm.MediaType)
            .Include(m => m.MovieFilmGenres)
                .ThenInclude(mf => mf.FilmGenre)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Return null if movie doesn't exist or user doesn't own it
        if (movie == null || movie.OwnerId != userId)
            return null;

        return movie;
    }

    public async Task<Movie?> CreateAsync(Movie movie)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        
        // Load related data after creation
        return await GetByIdAsync(movie.Id);
    }

    public async Task<Movie?> UpdateAsync(Guid id, Movie movie)
    {
        var existingMovie = await _context.Movies
            .Include(m => m.MovieMediaTypes)
            .Include(m => m.MovieFilmGenres)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (existingMovie == null)
            return null;

        existingMovie.Title = movie.Title;
        existingMovie.ReleaseYear = movie.ReleaseYear;
        
        // Update many-to-many relationships
        if (movie.MovieMediaTypes != null)
        {
            existingMovie.MovieMediaTypes.Clear();
            foreach (var mmt in movie.MovieMediaTypes)
            {
                existingMovie.MovieMediaTypes.Add(mmt);
            }
        }
        
        if (movie.MovieFilmGenres != null)
        {
            existingMovie.MovieFilmGenres.Clear();
            foreach (var mfg in movie.MovieFilmGenres)
            {
                existingMovie.MovieFilmGenres.Add(mfg);
            }
        }

        await _context.SaveChangesAsync();
        return await GetByIdAsync(existingMovie.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return false;

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteWithOwnershipCheckAsync(Guid id, string userId)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null || movie.OwnerId != userId)
            return false;

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        return true;
    }
}

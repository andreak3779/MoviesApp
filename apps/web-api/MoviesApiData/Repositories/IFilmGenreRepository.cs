using MoviesApiDomain.Models;

namespace MoviesApiData.Repositories;

public interface IFilmGenreRepository
{
    Task<IEnumerable<FilmGenre>> GetAllAsync();
    Task<FilmGenre?> GetByIdAsync(int id);
    Task<FilmGenre> CreateAsync(FilmGenre filmGenre);
    Task<FilmGenre?> UpdateAsync(int id, FilmGenre filmGenre);
    Task<bool> DeleteAsync(int id);
}

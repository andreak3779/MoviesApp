using MoviesApiDomain.Models;

namespace MoviesApiData.Repositories;

public interface IFilmGenreRepository
{
    Task<IEnumerable<FilmGenre>> GetAllAsync();
    Task<FilmGenre?> GetByIdAsync(int id);
}

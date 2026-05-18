using MoviesApi.Models;

namespace MoviesApi.Repositories;

public interface IFilmGenreRepository
{
    Task<IEnumerable<FilmGenre>> GetAllAsync();
    Task<FilmGenre?> GetByIdAsync(int id);
}

using MoviesApi.Models;

namespace MoviesApi.Repositories;

public interface IMovieRepository
{
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<IEnumerable<Movie>> GetAllByUserAsync(string userId);
    Task<(IEnumerable<Movie> Movies, int TotalCount)> GetPagedByUserAsync(string userId, int pageNumber, int pageSize);
    Task<Movie?> GetByIdAsync(Guid id);
    Task<Movie?> GetByIdWithOwnershipCheckAsync(Guid id, string userId);
    Task<Movie?> CreateAsync(Movie movie);
    Task<Movie?> UpdateAsync(Guid id, Movie movie);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteWithOwnershipCheckAsync(Guid id, string userId);
}

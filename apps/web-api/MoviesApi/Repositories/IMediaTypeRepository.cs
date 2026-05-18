using MoviesApi.Models;

namespace MoviesApi.Repositories;

public interface IMediaTypeRepository
{
    Task<IEnumerable<MediaType>> GetAllAsync();
    Task<MediaType?> GetByIdAsync(int id);
}

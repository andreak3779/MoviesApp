using MoviesApiDomain.Models;

namespace MoviesApiData.Repositories;

public interface IMediaTypeRepository
{
    Task<IEnumerable<MediaType>> GetAllAsync();
    Task<MediaType?> GetByIdAsync(int id);
}

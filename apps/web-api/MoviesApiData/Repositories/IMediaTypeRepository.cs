using MoviesApiDomain.Models;

namespace MoviesApiData.Repositories;

public interface IMediaTypeRepository
{
    Task<IEnumerable<MediaType>> GetAllAsync();
    Task<MediaType?> GetByIdAsync(int id);
    Task<MediaType> CreateAsync(MediaType mediaType);
    Task<MediaType?> UpdateAsync(int id, MediaType mediaType);
    Task<bool> DeleteAsync(int id);
}

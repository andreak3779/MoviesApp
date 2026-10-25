using MoviesApiDomain.Models;

namespace MoviesApiData.Repositories;

public interface IFormatRepository
{
    Task<IEnumerable<Format>> GetAllAsync();
    Task<Format?> GetByIdAsync(int id);
    Task<Format> CreateAsync(Format format);
    Task<Format?> UpdateAsync(int id, Format format);
    Task<bool> DeleteAsync(int id);
}

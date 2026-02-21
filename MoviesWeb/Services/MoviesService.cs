namespace MoviesWeb.Services;

public class MoviesService : IMoviesService
{
    public MoviesService(HttpClient httpClient) => _httpClient = httpClient;

    private readonly HttpClient _httpClient;

    public async Task<IEnumerable<Movie>> GetMoviesAsync()
    {
        var movies = await _httpClient.GetFromJsonAsync<IEnumerable<Movie>>("movies");
        return movies ?? Array.Empty<Movie>();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

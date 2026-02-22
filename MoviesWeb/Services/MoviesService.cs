using System;
using System.Linq;
using System.Net.Http.Json;

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

    public async Task<Movie> GetMovieByQueryAsync(MovieQuery query)
    {
        query.Validate();
        var qs = string.Join('&', query.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        var movie = await _httpClient.GetFromJsonAsync<Movie>($"movies/query?{qs}");
        return movie ?? new Movie();
    }
    public async Task<Movie> GetMovieAsync(int id)
    {
        var movie = await _httpClient.GetFromJsonAsync<Movie>($"movies/{id}");
        return movie ?? new Movie();
    }

    public async Task DeleteMovieAsync(int id)
    {
        using var response = await _httpClient.DeleteAsync($"movies/{id}");
        if (!response.IsSuccessStatusCode)
        {
            throw new System.Net.Http.HttpRequestException(
                $"Failed to delete movie with id {id}. Status code: {response.StatusCode}");

        }
        response.EnsureSuccessStatusCode();
    }   

    public async Task<HttpResponseMessage> UpdateMovieAsync(Movie movie) {
        var response = await _httpClient.PostAsJsonAsync("movies", movie);
        if (!response.IsSuccessStatusCode)        {
            throw new System.Net.Http.HttpRequestException(
                $"Failed to update movie with id {movie.Id}. Status code: {response.StatusCode}");
        }
        
        return response.EnsureSuccessStatusCode();
    }  

    public async Task CreateMovieAsync(Movie movie)
    {
        await _httpClient.PostAsJsonAsync("movies", movie);
    }
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

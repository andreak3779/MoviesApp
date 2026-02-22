namespace MoviesWeb.Services;

public interface IMoviesService : IAsyncDisposable
{
    /// <summary>Gets all movies.</summary>
    /// <returns>A list of movies.</returns>
    Task<IEnumerable<Movie>> GetMoviesAsync();

    /// <summary>Gets a movie that matches the provided query.</summary>
    /// <param name="query">The query criteria to apply.</param>
    /// <returns>The matching movie, or an empty movie if none is found.</returns>
    Task<Movie> GetMovieByQueryAsync(MovieQuery query);

    /// <summary>Gets a movie by its identifier.</summary>
    /// <param name="id">The movie identifier.</param>
    /// <returns>The matching movie, or an empty movie if none is found.</returns>
    Task<Movie> GetMovieAsync(int id);

    /// <summary>Deletes a movie by its identifier.</summary>
    /// <param name="id">The movie identifier.</param>
    Task DeleteMovieAsync(int id);

    /// <summary>Updates an existing movie.</summary>
    /// <param name="movie">The movie to update.</param>
    Task UpdateMovieAsync(Movie movie);

    /// <summary>Creates a new movie.</summary>
    /// <param name="movie">The movie to create.</param>
    Task CreateMovieAsync(Movie movie);
}

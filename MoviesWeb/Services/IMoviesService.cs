namespace MoviesWeb.Services;

public interface IMoviesService : IAsyncDisposable
{
    /// <summary>Gets all movies.</summary>
    /// <returns>A list of movies.</returns>
    Task<IEnumerable<Movie>> GetMoviesAsync();

    /// <summary>Gets a movie that matches the provided query.</summary>
    /// <param name="query">The query criteria to apply.</param>
    /// <returns>The matching movie.</returns>
    Task<Movie> GetMovieByQueryAsync(MovieQuery query);

    /// <summary>Gets a movie by its identifier.</summary>
    /// <param name="id">The movie identifier.</param>
    /// <returns>The matching movie.</returns>
    Task<Movie> GetMovieAsync(int id);

    /// <summary>Deletes a movie by its identifier.</summary>
    /// <param name="id">The movie identifier.</param>
    Task DeleteMovieAsync(int id);

    /// <summary>Updates an existing movie.</summary>
    /// <param name="movie">
    /// The movie to update. The movie is identified by its identifier property
    /// (for example, <c>Id</c>), which must correspond to an existing movie.
    /// </param>
    /// <remarks>
    /// If no existing movie with the same identifier is found, no update is performed and
    /// the method completes without modifying any movies. If the update fails for any other
    /// reason (for example, a persistence error), the returned task will be faulted with an
    /// appropriate exception describing the failure.
    /// </remarks>
    Task UpdateMovieAsync(Movie movie);

    /// <summary>Creates a new movie.</summary>
    /// <param name="movie">The movie to create.</param>
    Task CreateMovieAsync(Movie movie);
}

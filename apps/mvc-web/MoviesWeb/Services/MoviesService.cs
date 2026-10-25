namespace MoviesWeb.Services
{
    using System.Net;
    using System.Net.Http.Json;
    using MoviesWeb.Models;

    /// <summary>
    /// HTTP client for the Web API's movie endpoints.
    /// </summary>
    public interface IMoviesService : IAsyncDisposable
    {
        /// <summary>
        /// Gets a page of the caller's movies.
        /// </summary>
        /// <param name="query">Optional filter / sort / paging criteria.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The paged response.</returns>
        Task<PagedResponse<Movie>> GetMoviesAsync(MovieQuery? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single movie by its GUID identifier.
        /// </summary>
        /// <param name="id">The movie's GUID identifier.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The movie, or <c>null</c> when it does not exist or is not owned by the caller.</returns>
        Task<Movie?> GetMovieAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Deletes a movie by its GUID identifier.</summary>
        /// <param name="id">The movie's GUID identifier.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing movie.</summary>
        /// <param name="movie">The movie to update, including its identifier.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        Task UpdateMovieAsync(Movie movie, CancellationToken cancellationToken = default);

        /// <summary>Creates a new movie.</summary>
        /// <param name="movie">The movie to create.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The created movie.</returns>
        Task<Movie?> CreateMovieAsync(Movie movie, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of film genres used by the filter UI.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The available genres.</returns>
        Task<IReadOnlyList<FilmGenreDto>> GetFilmGenresAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of media types used by the filter UI.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The available media types.</returns>
        Task<IReadOnlyList<MediaTypeDto>> GetMediaTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of formats used by the filter UI.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The available formats.</returns>
        Task<IReadOnlyList<FormatDto>> GetFormatsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Talks to the Web API's <c>/api/v1</c> endpoints.
    /// </summary>
    /// <remarks>
    /// The API routes movies under <c>/api/v1/Movie</c> (the singular
    /// controller name) and identifies them by GUID. Every call except the
    /// anonymous lookups requires a bearer token, which
    /// <see cref="ApiTokenHandler"/> attaches.
    /// </remarks>
    public class MoviesService : IMoviesService
    {
        private const string MoviesPath = "Movie";
        private const string FilmGenresPath = "filmGenres";
        private const string MediaTypesPath = "mediaTypes";
        private const string FormatsPath = "formats";

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoviesService"/> class.
        /// </summary>
        /// <param name="httpClient">Client configured with the API base address.</param>
        public MoviesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task<PagedResponse<Movie>> GetMoviesAsync(
            MovieQuery? query = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(BuildMoviesUrl(query), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BuildExceptionAsync(response, "load movies");
            }

            var paged = await response.Content.ReadFromJsonAsync<PagedResponse<Movie>>(
                ApiJson.Options, cancellationToken);

            return paged ?? new PagedResponse<Movie>();
        }

        /// <inheritdoc />
        public async Task<Movie?> GetMovieAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{MoviesPath}/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw await BuildExceptionAsync(response, $"load movie {id}");
            }

            return await response.Content.ReadFromJsonAsync<Movie>(ApiJson.Options, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync($"{MoviesPath}/{id}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BuildExceptionAsync(response, $"delete movie {id}");
            }
        }

        /// <inheritdoc />
        public async Task UpdateMovieAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            var payload = new MovieUpdateRequest
            {
                Title = movie.Title,
                ReleaseYear = movie.ReleaseYear,
                Description = movie.Description,
                Favourite = movie.Favourite,
                Criterion = movie.Criterion,
                CriterionNumber = movie.CriterionNumber,
                FilmGenreId = movie.FilmGenres.FirstOrDefault()?.Id,
                MediaTypeId = movie.MediaTypes.FirstOrDefault()?.Id,
                FormatId = movie.Formats.FirstOrDefault()?.Id
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"{MoviesPath}/{movie.Id}", payload, ApiJson.Options, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BuildExceptionAsync(response, $"update movie {movie.Id}");
            }
        }

        /// <inheritdoc />
        public async Task<Movie?> CreateMovieAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            var payload = new MovieCreateRequest
            {
                Title = movie.Title,
                ReleaseYear = movie.ReleaseYear,
                Description = movie.Description,
                Favourite = movie.Favourite,
                Criterion = movie.Criterion,
                CriterionNumber = movie.CriterionNumber,
                FilmGenreId = movie.FilmGenres.FirstOrDefault()?.Id,
                MediaTypeId = movie.MediaTypes.FirstOrDefault()?.Id,
                FormatId = movie.Formats.FirstOrDefault()?.Id
            };

            var response = await _httpClient.PostAsJsonAsync(
                MoviesPath, payload, ApiJson.Options, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BuildExceptionAsync(response, "create movie");
            }

            return await response.Content.ReadFromJsonAsync<Movie>(ApiJson.Options, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<FilmGenreDto>> GetFilmGenresAsync(CancellationToken cancellationToken = default) =>
            GetLookupAsync<FilmGenreDto>(FilmGenresPath, cancellationToken);

        /// <inheritdoc />
        public Task<IReadOnlyList<MediaTypeDto>> GetMediaTypesAsync(CancellationToken cancellationToken = default) =>
            GetLookupAsync<MediaTypeDto>(MediaTypesPath, cancellationToken);

        /// <inheritdoc />
        public Task<IReadOnlyList<FormatDto>> GetFormatsAsync(CancellationToken cancellationToken = default) =>
            GetLookupAsync<FormatDto>(FormatsPath, cancellationToken);

        /// <summary>
        /// Releases the underlying HTTP client.
        /// </summary>
        public ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            return ValueTask.CompletedTask;
        }

        private async Task<IReadOnlyList<T>> GetLookupAsync<T>(string path, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BuildExceptionAsync(response, $"load {path}");
            }

            var items = await response.Content.ReadFromJsonAsync<List<T>>(ApiJson.Options, cancellationToken);
            return items ?? new List<T>();
        }

        private static string BuildMoviesUrl(MovieQuery? query)
        {
            if (query is null)
            {
                return MoviesPath;
            }

            query.Validate();
            var parts = query.ToQueryStringParts().ToList();
            return parts.Count == 0
                ? MoviesPath
                : $"{MoviesPath}?{string.Join("&", parts)}";
        }

        private static async Task<HttpRequestException> BuildExceptionAsync(
            HttpResponseMessage response,
            string operation)
        {
            var detail = await response.Content.ReadAsStringAsync();
            var message = $"Failed to {operation}. Status code: {(int)response.StatusCode} {response.StatusCode}";

            if (!string.IsNullOrWhiteSpace(detail))
            {
                message += $" - {detail}";
            }

            return new HttpRequestException(message);
        }
    }
}

namespace MoviesWeb.Controllers
{
    using System.Globalization;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using MoviesWeb.Models;
    using MoviesWeb.Services;
    using MoviesWeb.ViewModels;

    /// <summary>
    /// Serves the movie list, the add / edit form and the details page.
    /// </summary>
    [Authorize]
    public class MoviesController : Controller
    {
        private const int DefaultPageSize = 20;

        private readonly IMoviesService _moviesService;
        private readonly ILogger<MoviesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoviesController"/> class.
        /// </summary>
        /// <param name="moviesService">Service used to reach the Web API.</param>
        /// <param name="logger">Logger for API failures.</param>
        public MoviesController(IMoviesService moviesService, ILogger<MoviesController> logger)
        {
            _moviesService = moviesService;
            _logger = logger;
        }

        /// <summary>
        /// Shows a filtered, sorted, paged list of the signed-in user's movies.
        /// </summary>
        /// <param name="search">Free-text title filter.</param>
        /// <param name="genre">Genre name filter.</param>
        /// <param name="mediaType">Media type name filter.</param>
        /// <param name="format">Format name filter.</param>
        /// <param name="sortBy">Sort key: <c>title</c> or <c>year</c>.</param>
        /// <param name="sortDir">Sort direction: <c>asc</c> or <c>desc</c>.</param>
        /// <param name="page">1-based page number.</param>
        /// <returns>The movies list view.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            string? genre = null,
            string? mediaType = null,
            string? format = null,
            string? sortBy = null,
            string? sortDir = null,
            int page = 1)
        {
            var effectiveSortBy = string.IsNullOrWhiteSpace(sortBy) ? "year" : sortBy;
            var effectiveSortDir = string.IsNullOrWhiteSpace(sortDir) ? "desc" : sortDir;

            var query = new MovieQuery();
            query.Add("search", search);
            query.Add("genre", genre);
            query.Add("mediaType", mediaType);
            query.Add("format", format);
            query.Add("sortBy", effectiveSortBy);
            query.Add("sortDir", effectiveSortDir);
            query.Add("pageNumber", (page < 1 ? 1 : page).ToString(CultureInfo.InvariantCulture));
            query.Add("pageSize", DefaultPageSize.ToString(CultureInfo.InvariantCulture));

            PagedResponse<Movie> paged;
            IReadOnlyList<FilmGenreDto> genres;
            IReadOnlyList<MediaTypeDto> mediaTypes;
            IReadOnlyList<FormatDto> formats;

            try
            {
                paged = await _moviesService.GetMoviesAsync(query);
                var lookups = await LoadLookupsAsync();
                genres = lookups.Genres;
                mediaTypes = lookups.MediaTypes;
                formats = lookups.Formats;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to load movies from the Web API.");
                SetTempMessage("Error", "Could not reach the Movies API. Please try again.");
                return View("Index", new MovieListModel
                {
                    Movies = new List<Movie>(),
                    PageNumber = 1,
                    PageSize = DefaultPageSize,
                    TotalCount = 0,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false,
                    Genres = new List<FilmGenreDto>(),
                    MediaTypes = new List<MediaTypeDto>(),
                    Formats = new List<FormatDto>(),
                    Search = search,
                    Genre = genre,
                    MediaType = mediaType,
                    Format = format,
                    SortBy = effectiveSortBy,
                    SortDir = effectiveSortDir
                });
            }

            var model = new MovieListModel
            {
                Movies = paged.Items,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                HasPreviousPage = paged.HasPrevious,
                HasNextPage = paged.HasNext,
                Genres = genres.ToList(),
                MediaTypes = mediaTypes.ToList(),
                Formats = formats.ToList(),
                Search = search,
                Genre = genre,
                MediaType = mediaType,
                Format = format,
                SortBy = effectiveSortBy,
                SortDir = effectiveSortDir
            };

            return View("Index", model);
        }

        /// <summary>
        /// Shows the empty add-movie form.
        /// </summary>
        /// <returns>The form view.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new MovieFormModel();
            await FillLookupsAsync(model);

            return View("Create", model);
        }

        /// <summary>
        /// Adds a movie to the signed-in user's collection.
        /// </summary>
        /// <param name="model">The submitted form.</param>
        /// <returns>A redirect to the list, or the form with the errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillLookupsAsync(model);
                return View("Create", model);
            }

            try
            {
                await _moviesService.CreateMovieAsync(model.ToCreateRequest());
                SetTempMessage("Message", "Movie added.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to create a movie.");
                ModelState.AddModelError(string.Empty, "Could not reach the Movies API. Please try again.");
                await FillLookupsAsync(model);
                return View("Create", model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Shows the form for an existing movie.
        /// </summary>
        /// <param name="id">The movie's GUID identifier.</param>
        /// <returns>The form view, or 404 when the movie does not exist.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Movie? movie;
            try
            {
                movie = await _moviesService.GetMovieAsync(id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to load movie {Id} from the Web API.", id);
                return NotFound();
            }

            if (movie is null)
            {
                return NotFound();
            }

            var model = MovieFormModel.FromMovie(movie);
            await FillLookupsAsync(model);

            return View("Edit", model);
        }

        /// <summary>
        /// Saves the changes to an existing movie.
        /// </summary>
        /// <param name="id">The movie's GUID identifier.</param>
        /// <param name="model">The submitted form.</param>
        /// <returns>A redirect to the list, or the form with the errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, MovieFormModel model)
        {
            model.Id = id;

            if (!ModelState.IsValid)
            {
                await FillLookupsAsync(model);
                return View("Edit", model);
            }

            try
            {
                await _moviesService.UpdateMovieAsync(id, model.ToUpdateRequest());
                SetTempMessage("Message", "Movie updated.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to update movie {Id}.", id);
                ModelState.AddModelError(string.Empty, "Could not reach the Movies API. Please try again.");
                await FillLookupsAsync(model);
                return View("Edit", model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Shows the details page for a single movie.
        /// </summary>
        /// <param name="id">The movie's GUID identifier.</param>
        /// <returns>The details view, or 404 when the movie does not exist.</returns>
        [HttpGet]
        public async Task<IActionResult> MovieDetails(Guid id)
        {
            Movie? movie;
            try
            {
                movie = await _moviesService.GetMovieAsync(id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to load movie {Id} from the Web API.", id);
                return NotFound();
            }

            if (movie is null)
            {
                return NotFound();
            }

            return View("MovieDetails", movie);
        }

        /// <summary>
        /// Deletes a movie owned by the signed-in user.
        /// </summary>
        /// <param name="id">The movie's GUID identifier.</param>
        /// <returns>A redirect back to the list.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _moviesService.DeleteMovieAsync(id);
                SetTempMessage("Message", "Movie deleted.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to delete movie {Id}.", id);
                SetTempMessage("Error", "Could not delete the movie. Please try again.");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Loads the three lookup lists the filter row and the form offer.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the requests.</param>
        /// <returns>The lookup lists.</returns>
        private async Task<(IReadOnlyList<FilmGenreDto> Genres, IReadOnlyList<MediaTypeDto> MediaTypes, IReadOnlyList<FormatDto> Formats)>
            LoadLookupsAsync(CancellationToken cancellationToken = default)
        {
            var genresTask = _moviesService.GetFilmGenresAsync(cancellationToken);
            var mediaTypesTask = _moviesService.GetMediaTypesAsync(cancellationToken);
            var formatsTask = _moviesService.GetFormatsAsync(cancellationToken);

            await Task.WhenAll(genresTask, mediaTypesTask, formatsTask);

            return (await genresTask, await mediaTypesTask, await formatsTask);
        }

        /// <summary>
        /// Fills the form's lookup lists, so the selects can be rendered.
        /// </summary>
        /// <param name="model">The form model.</param>
        /// <param name="cancellationToken">Token to cancel the requests.</param>
        /// <returns>A task that completes when the lists have been loaded.</returns>
        private async Task FillLookupsAsync(MovieFormModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var lookups = await LoadLookupsAsync(cancellationToken);
                model.Genres = lookups.Genres.ToList();
                model.MediaTypes = lookups.MediaTypes.ToList();
                model.Formats = lookups.Formats.ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to load the lookup lists from the Web API.");
            }
        }

        /// <summary>
        /// Writes a flash message when the controller has a temp-data store.
        /// </summary>
        /// <remarks>
        /// TempData is unavailable when the controller is constructed directly
        /// (for example in unit tests), so the write is guarded rather than
        /// throwing a null reference.
        /// </remarks>
        private void SetTempMessage(string key, string message)
        {
            if (TempData is not null)
            {
                TempData[key] = message;
            }
        }
    }
}

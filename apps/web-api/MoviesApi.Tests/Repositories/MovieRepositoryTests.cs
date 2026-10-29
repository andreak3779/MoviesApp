using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MoviesApiData.Data;
using MoviesApiData.Repositories;
using MoviesApiDomain.Models;
using Xunit;

namespace MoviesApi.Tests.Repositories;

/// <summary>
/// Regression tests for <see cref="MovieRepository"/>. These cover the three
/// defects recorded as G25, G26 and G27:
/// <list type="bullet">
///   <item>G25 - <c>GetByIdAsync</c> did not include <c>MovieFormats</c>, so a
///   movie created with a format surfaced a null <c>Format</c> navigation.</item>
///   <item>G26 - <c>UpdateAsync</c> did not copy the scalar fields, and it
///   cleared each link collection and refilled it from an argument that could
///   be the very same tracked instance, which emptied the collection before it
///   was read.</item>
///   <item>G27 - <c>GetByIdWithOwnershipCheckAsync</c> did not include
///   <c>MovieFormats</c>, so the detail endpoint disagreed with the list
///   endpoint.</item>
/// </list>
/// <para>
/// Every test uses a private SQLite file and asserts through a fresh
/// <see cref="MoviesDbContext"/>. A fresh context is essential: within a
/// single context EF's identity resolution returns the already-tracked
/// instance, complete with navigation properties, which would mask a missing
/// <c>Include</c> and make the G27 test pass against unfixed code.
/// </para>
/// </summary>
public class MovieRepositoryTests : IDisposable
{
    private const string OwnerId = "test-owner-1";
    private const string OtherOwnerId = "test-owner-2";

    // Seeded by MoviesDbContext.OnModelCreating.
    private const int BluRayMediaTypeId = 2;
    private const int DigitalDownloadMediaTypeId = 4;
    private const int ScienceFictionGenreId = 6;
    private const int MovieFormatId = 1;

    private readonly string _databasePath;
    private readonly DbContextOptions<MoviesDbContext> _options;

    public MovieRepositoryTests()
    {
        _databasePath = Path.Combine(
            Path.GetTempPath(),
            $"moviesapi-repo-tests-{Guid.NewGuid():N}.db");

        _options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        using var context = new MoviesDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var path = _databasePath + suffix;
            if (!File.Exists(path))
                continue;

            try { File.Delete(path); }
            catch (IOException) { /* best effort */ }
        }
    }

    /// <summary>
    /// G25: creating a movie with a format must return the format hydrated.
    /// <c>CreateAsync</c> re-reads through <c>GetByIdAsync</c>, so this is the
    /// exact production path that used to throw a <see cref="NullReferenceException"/>.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WithFormat_ReturnsMovieWithFormatHydrated()
    {
        SeedOwner(OwnerId);

        using var context = new MoviesDbContext(_options);
        var repository = new MovieRepository(context);

        var movie = new Movie
        {
            Title = "Arrival",
            ReleaseYear = 2016,
            Description = "A linguist meets aliens.",
            OwnerId = OwnerId,
            MovieMediaTypes = { new MovieMediaType { MediaTypeId = BluRayMediaTypeId } },
            MovieFilmGenres = { new MovieFilmGenre { FilmGenreId = ScienceFictionGenreId } },
            MovieFormats = { new MovieFormat { FormatId = MovieFormatId } },
        };

        var created = await repository.CreateAsync(movie);

        Assert.NotNull(created);
        var format = Assert.Single(created!.MovieFormats);
        Assert.Equal(MovieFormatId, format.FormatId);
        Assert.NotNull(format.Format);
        Assert.Equal("Movie", format.Format.Name);
    }

    /// <summary>
    /// G25: the single-movie read must hydrate every link collection, so it
    /// agrees with the list endpoint.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsAllThreeCollectionsHydrated()
    {
        var movie = SeedMovieWithAllLinks();

        using var context = new MoviesDbContext(_options);
        var repository = new MovieRepository(context);

        var loaded = await repository.GetByIdAsync(movie.Id);

        Assert.NotNull(loaded);
        Assert.NotNull(loaded!.MovieMediaTypes.Single().MediaType);
        Assert.NotNull(loaded.MovieFilmGenres.Single().FilmGenre);
        Assert.NotNull(loaded.MovieFormats.Single().Format);
        Assert.Equal("Movie", loaded.MovieFormats.Single().Format.Name);
    }

    /// <summary>
    /// G27: the ownership-checked read must hydrate the formats too. Before
    /// the fix this returned an empty formats collection while the list
    /// endpoint returned the format.
    /// </summary>
    [Fact]
    public async Task GetByIdWithOwnershipCheckAsync_ReturnsFormatsHydrated()
    {
        var movie = SeedMovieWithAllLinks();

        using var context = new MoviesDbContext(_options);
        var repository = new MovieRepository(context);

        var loaded = await repository.GetByIdWithOwnershipCheckAsync(movie.Id, OwnerId);

        Assert.NotNull(loaded);
        var format = Assert.Single(loaded!.MovieFormats);
        Assert.Equal(MovieFormatId, format.FormatId);
        Assert.NotNull(format.Format);
        Assert.Equal("Movie", format.Format.Name);
    }

    /// <summary>
    /// The ownership check must still reject a movie the caller does not own.
    /// </summary>
    [Fact]
    public async Task GetByIdWithOwnershipCheckAsync_DifferentOwner_ReturnsNull()
    {
        var movie = SeedMovieWithAllLinks();
        SeedOwner(OtherOwnerId);

        using var context = new MoviesDbContext(_options);
        var repository = new MovieRepository(context);

        var loaded = await repository.GetByIdWithOwnershipCheckAsync(movie.Id, OtherOwnerId);

        Assert.Null(loaded);
    }

    /// <summary>
    /// G26: a detached desired-state document must persist every scalar. The
    /// old implementation copied only the title and the release year, so the
    /// remaining fields were never written.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithDetachedEntity_PersistsAllScalars()
    {
        var movie = SeedMovieWithAllLinks();

        var desired = new Movie
        {
            Id = movie.Id,
            Title = "Arrival (2016)",
            ReleaseYear = 2016,
            Description = "A linguist meets aliens.",
            Favourite = true,
            Criterion = true,
            CriterionNumber = 42,
            OwnerId = OwnerId,
            UpdatedAt = DateTime.UtcNow,
        };

        using (var context = new MoviesDbContext(_options))
        {
            var repository = new MovieRepository(context);
            var result = await repository.UpdateAsync(movie.Id, desired);
            Assert.NotNull(result);
        }

        // Assert through a fresh context so the read comes from the database
        // rather than from the change tracker.
        using var assertContext = new MoviesDbContext(_options);
        var stored = await assertContext.Movies
            .Include(m => m.MovieMediaTypes).ThenInclude(mm => mm.MediaType)
            .Include(m => m.MovieFilmGenres).ThenInclude(mf => mf.FilmGenre)
            .Include(m => m.MovieFormats).ThenInclude(mf => mf.Format)
            .FirstAsync(m => m.Id == movie.Id);

        Assert.Equal("Arrival (2016)", stored.Title);
        Assert.Equal(2016, stored.ReleaseYear);
        Assert.Equal("A linguist meets aliens.", stored.Description);
        Assert.True(stored.Favourite);
        Assert.True(stored.Criterion);
        Assert.Equal(42, stored.CriterionNumber);
        Assert.NotNull(stored.UpdatedAt);
    }

    /// <summary>
    /// G26: only the collection the caller changed may be replaced. The other
    /// two must survive the update untouched.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReplacesOnlyTheCollectionThatChanged()
    {
        var movie = SeedMovieWithAllLinks();

        var desired = new Movie
        {
            Id = movie.Id,
            Title = movie.Title,
            ReleaseYear = movie.ReleaseYear,
            Description = movie.Description,
            Favourite = movie.Favourite,
            Criterion = movie.Criterion,
            CriterionNumber = movie.CriterionNumber,
            OwnerId = OwnerId,
            UpdatedAt = DateTime.UtcNow,
            MovieMediaTypes = new List<MovieMediaType>
            {
                new MovieMediaType { MediaTypeId = DigitalDownloadMediaTypeId },
            },
            MovieFilmGenres = new List<MovieFilmGenre>
            {
                new MovieFilmGenre { FilmGenreId = ScienceFictionGenreId },
            },
            MovieFormats = new List<MovieFormat>
            {
                new MovieFormat { FormatId = MovieFormatId },
            },
        };

        using (var context = new MoviesDbContext(_options))
        {
            var repository = new MovieRepository(context);
            var result = await repository.UpdateAsync(movie.Id, desired);
            Assert.NotNull(result);
        }

        using var assertContext = new MoviesDbContext(_options);
        var stored = await assertContext.Movies
            .Include(m => m.MovieMediaTypes)
            .Include(m => m.MovieFilmGenres)
            .Include(m => m.MovieFormats)
            .FirstAsync(m => m.Id == movie.Id);

        Assert.Equal(DigitalDownloadMediaTypeId, stored.MovieMediaTypes.Single().MediaTypeId);
        Assert.Contains(stored.MovieFilmGenres, fg => fg.FilmGenreId == ScienceFictionGenreId);
        Assert.Contains(stored.MovieFormats, f => f.FormatId == MovieFormatId);
    }

    /// <summary>
    /// The reported bug: a caller hands <c>UpdateAsync</c> the very instance
    /// the repository returned. The old implementation cleared the tracked
    /// collection and then refilled it from that same instance, which emptied
    /// the collection before it was read, so every link was lost.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithTrackedInstance_DoesNotWipeUntouchedCollections()
    {
        var movie = SeedMovieWithAllLinks();

        using var context = new MoviesDbContext(_options);
        var repository = new MovieRepository(context);

        // Load the movie the way the controller does.
        var tracked = await repository.GetByIdWithOwnershipCheckAsync(movie.Id, OwnerId);
        Assert.NotNull(tracked);

        // Mutate the tracked instance and change only the media types.
        tracked!.Title = "Arrival (2016)";
        tracked.MovieMediaTypes.Clear();
        tracked.MovieMediaTypes.Add(new MovieMediaType { MediaTypeId = DigitalDownloadMediaTypeId });

        // Hand that same instance back to the repository.
        var result = await repository.UpdateAsync(movie.Id, tracked);
        Assert.NotNull(result);

        using var assertContext = new MoviesDbContext(_options);
        var stored = await assertContext.Movies
            .Include(m => m.MovieMediaTypes)
            .Include(m => m.MovieFilmGenres)
            .Include(m => m.MovieFormats)
            .FirstAsync(m => m.Id == movie.Id);

        Assert.Equal("Arrival (2016)", stored.Title);
        Assert.Equal(DigitalDownloadMediaTypeId, stored.MovieMediaTypes.Single().MediaTypeId);
        Assert.Contains(stored.MovieFilmGenres, fg => fg.FilmGenreId == ScienceFictionGenreId);
        Assert.Contains(stored.MovieFormats, f => f.FormatId == MovieFormatId);
    }

    private void SeedOwner(string ownerId)
    {
        using var context = new MoviesDbContext(_options);

        if (context.Users.Any(u => u.Id == ownerId))
            return;

        context.Users.Add(new ApplicationUser
        {
            Id = ownerId,
            UserName = $"{ownerId}@example.test",
            NormalizedUserName = $"{ownerId.ToUpperInvariant()}@EXAMPLE.TEST",
            Email = $"{ownerId}@example.test",
            NormalizedEmail = $"{ownerId.ToUpperInvariant()}@EXAMPLE.TEST",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = true,
        });

        context.SaveChanges();
    }

    private Movie SeedMovieWithAllLinks()
    {
        SeedOwner(OwnerId);

        using var context = new MoviesDbContext(_options);

        var movie = new Movie
        {
            Title = "Arrival",
            ReleaseYear = 2016,
            Description = "A linguist meets aliens.",
            OwnerId = OwnerId,
            MovieMediaTypes = { new MovieMediaType { MediaTypeId = BluRayMediaTypeId } },
            MovieFilmGenres = { new MovieFilmGenre { FilmGenreId = ScienceFictionGenreId } },
            MovieFormats = { new MovieFormat { FormatId = MovieFormatId } },
        };

        context.Movies.Add(movie);
        context.SaveChanges();

        return movie;
    }
}

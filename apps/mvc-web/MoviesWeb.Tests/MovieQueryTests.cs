using MoviesWeb.Models;

namespace MoviesWeb.Tests.Models;

/// <summary>
/// Verifies that <see cref="MovieQuery"/> only accepts the keys the Web API
/// understands, so a caller cannot smuggle arbitrary parameters upstream.
/// </summary>
public class MovieQueryTests
{
    [Theory]
    [InlineData("search")]
    [InlineData("genre")]
    [InlineData("mediaType")]
    [InlineData("format")]
    [InlineData("sortBy")]
    [InlineData("sortDir")]
    [InlineData("pageNumber")]
    [InlineData("pageSize")]
    public void Add_AllowsApiQueryKeys(string key)
    {
        var query = new MovieQuery();

        query.Add(key, "value");

        Assert.Single(query.AsEnumerable());
    }

    [Theory]
    [InlineData("title")]
    [InlineData("director")]
    [InlineData("releaseYear")]
    [InlineData("anything")]
    public void Add_RejectsKeysTheApiDoesNotSupport(string key)
    {
        var query = new MovieQuery();

        var ex = Assert.Throws<ArgumentException>(() => query.Add(key, "value"));
        Assert.Contains("Invalid query key", ex.Message);
    }

    [Fact]
    public void Add_DropsBlankValues_SoTheApiAppliesItsOwnDefaults()
    {
        var query = new MovieQuery();

        query.Add("search", "   ");
        query.Add("genre", null);

        Assert.Empty(query.AsEnumerable());
    }

    [Fact]
    public void Add_ReplacesAnExistingValue()
    {
        var query = new MovieQuery();

        query.Add("search", "first");
        query.Add("search", "second");

        var parts = query.ToQueryStringParts().ToList();
        Assert.Single(parts);
        Assert.Equal("search=second", parts[0]);
    }

    [Fact]
    public void ToQueryStringParts_EscapesValues()
    {
        var query = new MovieQuery();
        query.Add("genre", "Science Fiction");

        var parts = query.ToQueryStringParts().ToList();
        Assert.Equal("genre=Science%20Fiction", parts[0]);
    }
}

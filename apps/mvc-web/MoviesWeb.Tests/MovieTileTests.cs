using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesWeb.Controllers;
using MoviesWeb.ViewModels;

namespace MoviesWeb.Tests.ViewModels;

/// <summary>
/// Verifies the poster tile: a stable colour per movie and the title's initial.
/// </summary>
public class MovieTileTests
{
    [Fact]
    public void Color_IsStableForTheSameMovie()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        Assert.Equal(MovieTile.Color(id), MovieTile.Color(id));
    }

    [Fact]
    public void Color_ComesFromTheMockupPalette()
    {
        var palette = new[]
        {
            "oklch(40% 0.19 350)",
            "oklch(42% 0.17 320)",
            "oklch(44% 0.15 295)",
            "oklch(46% 0.14 270)",
            "oklch(43% 0.16 10)"
        };

        for (var i = 0; i < 25; i++)
        {
            Assert.Contains(MovieTile.Color(Guid.NewGuid()), palette);
        }
    }

    [Theory]
    [InlineData("Inception", "I")]
    [InlineData("  inception", "I")]
    [InlineData("dune", "D")]
    public void Letter_ReturnsTheFirstLetter(string title, string expected)
    {
        Assert.Equal(expected, MovieTile.Letter(title));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Letter_FallsBackToAQuestionMark(string? title)
    {
        Assert.Equal("?", MovieTile.Letter(title));
    }
}

/// <summary>
/// Verifies the static pages: the home page, the privacy notice and the error page.
/// </summary>
public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsTheHomeView()
    {
        var controller = new HomeController();

        var result = Assert.IsType<ViewResult>(controller.Index());

        Assert.Null(result.ViewName);
    }

    [Fact]
    public void Privacy_ReturnsThePrivacyView()
    {
        var controller = new HomeController();

        var result = Assert.IsType<ViewResult>(controller.Privacy());

        Assert.Null(result.ViewName);
    }

    [Fact]
    public void Error_ReturnsTheErrorView_WithARequestId()
    {
        var controller = new HomeController
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = Assert.IsType<ViewResult>(controller.Error());

        var model = Assert.IsType<ErrorViewModel>(result.Model);
        Assert.False(string.IsNullOrEmpty(model.RequestId));
        Assert.True(model.ShowRequestId);
    }
}

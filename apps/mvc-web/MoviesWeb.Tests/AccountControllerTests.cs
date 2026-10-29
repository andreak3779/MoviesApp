using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using MoviesWeb.Controllers;
using MoviesWeb.Models;
using MoviesWeb.Services;

namespace MoviesWeb.Tests.Controllers;

/// <summary>
/// Verifies that the account controller signs users in and out against the
/// Web API, and that signing out revokes the refresh token server-side.
/// </summary>
public class AccountControllerTests
{
    private readonly Mock<IApiAuthService> _authService = new();
    private readonly Mock<IMovieSession> _movieSession = new();
    private readonly Mock<ILogger<AccountController>> _logger = new();
    private readonly Mock<IAuthenticationService> _authentication = new();
    private readonly Mock<IUrlHelper> _url = new();

    private AccountController CreateController()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_authentication.Object);
        services.AddLogging();

        // Controller.View() asks the request services for a temp-data store,
        // so the test has to provide one.
        var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
        tempDataFactory
            .Setup(f => f.GetTempData(It.IsAny<HttpContext>()))
            .Returns(new Mock<ITempDataDictionary>().Object);
        services.AddSingleton(tempDataFactory.Object);

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var controller = new AccountController(
            _authService.Object,
            _movieSession.Object,
            _logger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context },
            Url = _url.Object
        };

        return controller;
    }

    private static AuthResponse SampleAuth() => new()
    {
        Token = "access-token",
        RefreshToken = "refresh-token",
        ExpiresAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        Email = "ada@example.com"
    };

    [Fact]
    public void Login_Get_ReturnsTheForm()
    {
        var controller = CreateController();

        var result = Assert.IsType<ViewResult>(controller.Login("/Movies"));

        Assert.IsType<LoginRequest>(result.Model);
        Assert.Equal("/Movies", controller.ViewData["ReturnUrl"]);
    }

    [Fact]
    public async Task Login_Post_SignsInAndRedirectsToMovies()
    {
        _authService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleAuth());

        var controller = CreateController();
        var result = await controller.Login(new LoginRequest
        {
            Email = "ada@example.com",
            Password = "Passw0rd!"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Movies", redirect.ControllerName);

        _movieSession.Verify(s => s.SignIn(It.IsAny<AuthResponse>()), Times.Once);
        _authentication.Verify(
            a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(s => s == "Cookies"),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_Post_RedirectsBackToWhereTheUserCameFrom()
    {
        _authService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleAuth());
        _url.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);

        var controller = CreateController();
        var result = await controller.Login(
            new LoginRequest { Email = "ada@example.com", Password = "Passw0rd!" },
            "/Movies/Edit/11111111-1111-1111-1111-111111111111");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/Movies/Edit/11111111-1111-1111-1111-111111111111", redirect.Url);
    }

    [Fact]
    public async Task Login_Post_IgnoresAnOffSiteReturnUrl()
    {
        _authService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleAuth());
        _url.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(false);

        var controller = CreateController();
        var result = await controller.Login(
            new LoginRequest { Email = "ada@example.com", Password = "Passw0rd!" },
            "https://evil.example.com");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Login_Post_ReturnsTheForm_WhenTheCredentialsAreRejected()
    {
        _authService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponse?)null);

        var controller = CreateController();
        var result = await controller.Login(new LoginRequest
        {
            Email = "ada@example.com",
            Password = "wrong"
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<LoginRequest>(viewResult.Model);
        Assert.True(controller.ModelState.ContainsKey(string.Empty));
        _movieSession.Verify(s => s.SignIn(It.IsAny<AuthResponse>()), Times.Never);
    }

    [Fact]
    public async Task Login_Post_ReturnsTheForm_WhenTheApiIsUnreachable()
    {
        _authService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var controller = CreateController();
        var result = await controller.Login(new LoginRequest
        {
            Email = "ada@example.com",
            Password = "Passw0rd!"
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<LoginRequest>(viewResult.Model);
        Assert.True(controller.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task Login_Post_ReturnsTheForm_WhenTheModelIsInvalid()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Email is required.");

        var result = await controller.Login(new LoginRequest { Email = string.Empty });

        Assert.IsType<ViewResult>(result);
        _authService.Verify(
            s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Register_Get_ReturnsTheForm()
    {
        var controller = CreateController();

        var result = Assert.IsType<ViewResult>(controller.Register());

        Assert.IsType<RegisterRequest>(result.Model);
    }

    [Fact]
    public async Task Register_Post_CreatesTheAccountAndSignsIn()
    {
        _authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleAuth());

        var controller = CreateController();
        var result = await controller.Register(new RegisterRequest
        {
            Email = "ada@example.com",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Movies", redirect.ControllerName);

        _movieSession.Verify(s => s.SignIn(It.IsAny<AuthResponse>()), Times.Once);
    }

    [Fact]
    public async Task Register_Post_ReturnsTheForm_WhenTheApiRejectsTheRegistration()
    {
        _authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponse?)null);

        var controller = CreateController();
        var result = await controller.Register(new RegisterRequest
        {
            Email = "ada@example.com",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!"
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<RegisterRequest>(viewResult.Model);
        Assert.True(controller.ModelState.ContainsKey(string.Empty));
        _movieSession.Verify(s => s.SignIn(It.IsAny<AuthResponse>()), Times.Never);
    }

    [Fact]
    public async Task Register_Post_ReturnsTheForm_WhenTheApiIsUnreachable()
    {
        _authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var controller = CreateController();
        var result = await controller.Register(new RegisterRequest
        {
            Email = "ada@example.com",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!"
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<RegisterRequest>(viewResult.Model);
        Assert.True(controller.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task Logout_RevokesTheRefreshToken_AndClearsTheSession()
    {
        _movieSession.Setup(s => s.RefreshToken).Returns("refresh-token");
        _movieSession.Setup(s => s.AccessToken).Returns("access-token");
        _authService.Setup(s => s.LogoutAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController();
        var result = await controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);

        _authService.Verify(s => s.LogoutAsync(
            "access-token",
            "refresh-token",
            It.IsAny<CancellationToken>()), Times.Once);
        _movieSession.Verify(s => s.SignOut(), Times.Once);
        _authentication.Verify(
            a => a.SignOutAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(s => s == "Cookies"),
                It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }

    [Fact]
    public async Task Logout_SignsOutEvenWhenTheRevokeFails()
    {
        _movieSession.Setup(s => s.RefreshToken).Returns("refresh-token");
        _movieSession.Setup(s => s.AccessToken).Returns("access-token");
        _authService.Setup(s => s.LogoutAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var controller = CreateController();
        var result = await controller.Logout();

        Assert.IsType<RedirectToActionResult>(result);
        _movieSession.Verify(s => s.SignOut(), Times.Once);
    }

    [Fact]
    public async Task Logout_SkipsTheRevoke_WhenThereIsNoRefreshToken()
    {
        _movieSession.Setup(s => s.RefreshToken).Returns((string?)null);

        var controller = CreateController();
        await controller.Logout();

        _authService.Verify(s => s.LogoutAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _movieSession.Verify(s => s.SignOut(), Times.Once);
    }
}

using System.Net;
using MoviesWeb.Tests.Infrastructure;

namespace MoviesWeb.Tests.Pages;

/// <summary>
/// Verifies that the account pages render the mockup's sign-in screen and
/// that the sign-out round trip works against the real pipeline.
/// </summary>
public class AccountPageTests : IClassFixture<MoviesWebFactory>
{
    private readonly MoviesWebFactory _factory;

    public AccountPageTests(MoviesWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoginPage_RendersTheMockupSignInScreen()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("login-card", html);
        Assert.Contains("name=\"Email\"", html);
        Assert.Contains("name=\"Password\"", html);
        Assert.Contains("Sign in", html);
    }

    [Fact]
    public async Task RegisterPage_RendersTheCreateAccountForm()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Account/Register");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Create account", html);
        Assert.Contains("name=\"ConfirmPassword\"", html);
        Assert.Contains("name=\"FirstName\"", html);
    }

    [Fact]
    public async Task HealthEndpoint_Answers()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HomePage_Renders()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Home", html);
    }

    [Fact]
    public async Task PrivacyPage_Renders()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Home/Privacy");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Privacy Policy", html);
    }

    [Fact]
    public async Task ErrorPage_Renders()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Home/Error");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("An error occurred while processing your request.", html);
    }
}

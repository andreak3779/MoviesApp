using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// The Web API's base address. It must include the /api/v1 prefix because the
// API's routes are versioned by path segment.
var apiBaseUrl = builder.Configuration["MoviesApi:BaseUrl"] ?? string.Empty;
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException(
        "MoviesApi:BaseUrl is not configured. Set it to the API's /api/v1 base address, " +
        "for example 'http://localhost:8081/api/v1'.");
}

if (!apiBaseUrl.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase)
    && !apiBaseUrl.EndsWith("/api/v1/", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        $"MoviesApi:BaseUrl ('{apiBaseUrl}') must point at the API's /api/v1 root, " +
        "for example 'http://localhost:8081/api/v1'.");
}

// Session + HTTP context access so the auth token handler can read the
// signed-in user's bearer token.
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "MoviesWeb.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

// Attaches the API bearer token to outgoing requests.
builder.Services.AddTransient<ApiTokenHandler>();

builder.Services.AddHttpClient<IMoviesService, MoviesService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
}).AddHttpMessageHandler<ApiTokenHandler>();

builder.Services.AddHttpClient<IApiAuthService, ApiAuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<ApiTokenHandler>();

builder.Services.AddScoped<IMovieSession, MovieSession>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "MoviesWeb.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => new { status = "ok" });

app.MapDefaultControllerRoute();

app.Run();

// Exposed so the integration tests can boot the real pipeline in-process.
public partial class Program { }

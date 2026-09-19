using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoviesApiData.Data;
using MoviesApi.Middleware;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;
using MoviesApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

// Add DbContext with SQLite
builder.Services.AddDbContext<MoviesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<MoviesDbContext>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured");

// Fail fast on the placeholder key shipped with appsettings.json. The same
// guard is duplicated in AuthService so both code paths refuse the
// placeholder; see also .gitleaks.toml for the allowlist.
const string PlaceholderJwtKey =
    "CHANGE_THIS_TO_A_SECURE_RANDOM_STRING_MIN_32_CHARS_IN_PRODUCTION";
if (string.Equals(secretKey, PlaceholderJwtKey, StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "JwtSettings:SecretKey is still the placeholder value. " +
        "Set a real 32+ byte secret via user-secrets (dev) or an environment " +
        "variable (prod) before starting the API.");
}
if (Encoding.UTF8.GetByteCount(secretKey) < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:SecretKey must be at least 32 bytes when UTF-8 encoded.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireAuthenticatedUser());
});

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MoviesApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
// Origins come from configuration (Cors:AllowedOrigins). In Production we
// refuse to start if any origin matches the placeholder literal that the
// repository ships with, because AllowCredentials() makes an over-permissive
// origin list genuinely dangerous.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

if (builder.Environment.IsProduction())
{
    var placeholders = new[] { "https://your-production-domain.com", "http://localhost" };
    foreach (var origin in allowedOrigins)
    {
        foreach (var placeholder in placeholders)
        {
            if (origin.StartsWith(placeholder, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Cors:AllowedOrigins contains the placeholder value '{origin}'. " +
                    "Configure real origins before starting the API in Production.");
            }
        }
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add repositories
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMediaTypeRepository, MediaTypeRepository>();
builder.Services.AddScoped<IFilmGenreRepository, FilmGenreRepository>();

// Add Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Add Audit Log Service
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Add Rate Limiting
builder.Services.Configure<RateLimitOptions>(options =>
{
    options.GlobalLimit = 100; // 100 requests per minute for reads
    options.WriteLimit = 10; // 10 requests per minute for writes
    options.WindowInSeconds = 60;
});

var app = builder.Build();

// Ensure database is created and seeded with roles
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Seed roles
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }
}

var enableHttpsRedirection = app.Configuration.GetValue("EnableHttpsRedirection", true);

// Configure the HTTP request pipeline.
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    // Swagger is disabled in production for security
    // To enable temporarily, use: app.UseSwagger(); app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

/* // Define endpoints
app.MapGet("/movies", () => movies);

app.MapGet("/movies/{id}", (int id) => 
{
    var movie = movies.FirstOrDefault(m => m.Id == id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
});

app.MapPost("/movies", (Movie movie) => 
{
    movie.Id = movies.Count > 0 ? movies.Max(m => m.Id) + 1 : 1;
    movies.Add(movie);
    return Results.Created($"/movies/{movie.Id}", movie);
});

app.MapPut("/movies/{id}", (int id, Movie updatedMovie) => 
{
    var index = movies.FindIndex(m => m.Id == id);
    if (index == -1) return Results.NotFound();

    updatedMovie.Id = id;
    movies[index] = updatedMovie;
    return Results.NoContent();
});

app.MapDelete("/movies/{id}", (int id) => 
{
    var index = movies.FindIndex(m => m.Id == id);
    if (index == -1) return Results.NotFound();

    movies.RemoveAt(index);
    return Results.NoContent();
}); */

app.Run();
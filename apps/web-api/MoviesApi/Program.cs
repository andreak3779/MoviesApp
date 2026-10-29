using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoviesApiData.Data;
using MoviesApi.Middleware;
using MoviesApi.Configuration;
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
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MoviesApi",
        Version = "v1",
        Description = "MoviesApp Web API. All movie endpoints require a bearer token; " +
                      "sign in through POST /api/v1/Auth/login and paste the token into " +
                      "the Authorize dialog in the Swagger UI."
    });

    // Render the XML comments on the controllers and DTOs as endpoint and
    // schema descriptions. The file only exists when the project was built
    // with GenerateDocumentationFile, so the check keeps `dotnet run` working
    // against a published output that was trimmed.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // An http/bearer scheme (rather than apiKey) is what makes the Swagger
    // UI send `Authorization: Bearer <token>` on the Authorize button.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
    CorsOriginGuard.EnsureNoPlaceholders(allowedOrigins);
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
builder.Services.AddScoped<IFormatRepository, FormatRepository>();

// Add Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Add Audit Log Service
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Add Rate Limiting. The values are bound from the "RateLimiting" section so
// a deployment can tune them without a rebuild; when the section is absent the
// defaults on RateLimitOptions apply (100 reads and 10 writes per minute).
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("RateLimiting"));

// HSTS is applied by the framework's UseHsts middleware. The values are
// configured here because UseHsts takes no options in the minimal hosting
// model. SecurityHeadersMiddleware used to append the header as well, which
// meant production responses carried it twice.
builder.Services.Configure<HstsOptions>(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
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
//
// The order is load-bearing, and the reasons are not obvious from the list:
//
//   SecurityHeaders   - first, so every response (including the Swagger UI)
//                       carries the security headers.
//   ExceptionHandling - wraps everything below it, so a throw anywhere
//                       downstream becomes a ProblemDetails response.
//   Swagger           - before the rate limiter, so loading the UI and its
//                       static assets does not consume the API's request
//                       budget.
//   CORS              - before the rate limiter, so a 429 carries the
//                       Access-Control-Allow-Origin header. A browser
//                       cannot read an error it is not allowed to see.
//   Authentication    - before the rate limiter, so the limiter can key on
//                       the caller's id instead of falling back to the IP.
//   RateLimiting      - after auth and CORS, before the endpoints.
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger is on by default in Development and off in every other
// environment, so the container stack does not publish the API surface
// unless it is asked to. Set EnableSwagger=true to expose it anywhere.
var enableSwagger = app.Configuration.GetValue(
    "EnableSwagger", app.Environment.IsDevelopment());

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "MoviesApi - MoviesApp";
        options.DisplayRequestDuration();
    });
}

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseMiddleware<RateLimitingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    // HSTS is owned by the framework here. SecurityHeadersMiddleware used to
    // append the header as well, which meant production responses carried
    // it twice.
    app.UseHsts();
}

// Honour the toggle. The container image and docker-compose both set
// EnableHttpsRedirection=false because the stack serves plain HTTP behind a
// reverse proxy; calling UseHttpsRedirection unconditionally redirected every
// request to an https endpoint that does not exist.
if (enableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

app.Run();
// Exposes the top-level-statements Program class to the test project.
public partial class Program { }
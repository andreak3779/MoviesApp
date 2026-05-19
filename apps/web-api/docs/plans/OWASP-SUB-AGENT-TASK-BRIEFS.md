# OWASP Top 10 Compliance - Sub-Agent Task Briefs

This document contains independent task briefs for sub-agents to implement OWASP Top 10 API Security compliance for MoviesApi.

## How to Use

Each task is self-contained and can be assigned to a sub-agent. Tasks are grouped by phase but can be executed independently. Copy the entire task brief when invoking a sub-agent.

---

## Phase 1: Authentication Foundation

### Task 1-A: Install Security Packages

**Objective:** Add required NuGet packages for authentication and security

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/MoviesApi.csproj`

**Instructions:**
1. Read the current `MoviesApi.csproj` file
2. Add the following PackageReferences inside the existing `<ItemGroup>`:
   ```xml
   <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.*" />
   <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.*" />
   <PackageReference Include="Microsoft.AspNetCore.RateLimiting" Version="10.*" />
   <PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning" Version="10.*" />
   <PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer" Version="10.*" />
   <PackageReference Include="AutoMapper" Version="13.*" />
   <PackageReference Include="BCrypt.Net-Next" Version="4.*" />
   ```
3. Ensure packages are added within the existing `<ItemGroup>` tag (don't create a new one)

**Success Criteria:**
- All 7 packages added to `.csproj`
- File maintains valid XML structure
- Run `dotnet restore` to verify packages restore successfully

**Time Estimate:** 15 minutes

---

### Task 1-B: Create User Model and Identity Configuration

**Objective:** Set up ASP.NET Core Identity with a custom User model

**Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Models/ApplicationUser.cs`

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Data/MoviesDbContext.cs`

**Instructions:**

1. **Create `ApplicationUser.cs`:**
   ```csharp
   using Microsoft.AspNetCore.Identity;

   namespace MoviesApi.Models;

   public class ApplicationUser : IdentityUser
   {
       // Add custom properties if needed
       public string? FirstName { get; set; }
       public string? LastName { get; set; }
       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   }
   ```

2. **Update `MoviesDbContext.cs`:**
   - Add `using Microsoft.AspNetCore.Identity.EntityFrameworkCore;`
   - Change base class from `DbContext` to `IdentityDbContext<ApplicationUser>`
   - Add constructor parameter for `IdentityDbContextOptions<ApplicationUser>`
   - Override `OnModelCreating` to call base implementation

**Success Criteria:**
- `ApplicationUser` class inherits from `IdentityUser`
- `MoviesDbContext` inherits from `IdentityDbContext<ApplicationUser>`
- Project builds successfully with `dotnet build`

**Time Estimate:** 30 minutes

---

### Task 1-C: Implement JWT Authentication Service

**Objective:** Create a service to handle JWT token generation and validation

**Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Services/AuthService.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/DTOs/AuthDtos.cs`

**Instructions:**

1. **Create `AuthDtos.cs`:**
   ```csharp
   namespace MoviesApi.DTOs;

   public class RegisterDto
   {
       public string Email { get; set; } = string.Empty;
       public string Password { get; set; } = string.Empty;
       public string? FirstName { get; set; }
       public string? LastName { get; set; }
   }

   public class LoginDto
   {
       public string Email { get; set; } = string.Empty;
       public string Password { get; set; } = string.Empty;
   }

   public class AuthResponseDto
   {
       public string Token { get; set; } = string.Empty;
       public string RefreshToken { get; set; } = string.Empty;
       public DateTime ExpiresAt { get; set; }
       public string Email { get; set; } = string.Empty;
   }

   public class RefreshTokenDto
   {
       public string Token { get; set; } = string.Empty;
       public string RefreshToken { get; set; } = string.Empty;
   }
   ```

2. **Create `AuthService.cs`:**
   - Inject `UserManager<ApplicationUser>`, `SignInManager<ApplicationUser>`, `IConfiguration`
   - Implement `RegisterAsync(RegisterDto dto)` method
   - Implement `LoginAsync(LoginDto dto)` method
   - Implement `RefreshTokenAsync(string token, string refreshToken)` method
   - Implement `GenerateJwtToken(ApplicationUser user)` method
   - Use BCrypt for password hashing (or ASP.NET Identity's built-in hashing)
   - JWT settings: 15-minute access token, 7-day refresh token
   - Store refresh tokens in database (create a RefreshTokens table/model)

**Additional Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Models/RefreshToken.cs`
  - Properties: Id, Token, UserId, ExpiresAt, IsExpired, RevokedAt

**Success Criteria:**
- AuthService compiles without errors
- JWT tokens are generated with correct expiration
- Refresh tokens are stored and validated
- Password validation enforces minimum 8 characters

**Time Estimate:** 2 hours

---

### Task 1-D: Create Authentication Controller

**Objective:** Expose authentication endpoints via a REST API controller

**Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/AuthController.cs`

**Instructions:**

1. **Create `AuthController.cs`:**
   ```csharp
   using Microsoft.AspNetCore.Mvc;
   using MoviesApi.DTOs;
   using MoviesApi.Services;

   namespace MoviesApi.Controllers;

   [ApiController]
   [Route("api/v1/[controller]")]
   public class AuthController : ControllerBase
   {
       private readonly AuthService _authService;

       public AuthController(AuthService authService)
       {
           _authService = authService;
       }

       [HttpPost("register")]
       public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
       {
           // Call authService.RegisterAsync
           // Handle errors (email already exists, weak password, etc.)
       }

       [HttpPost("login")]
       public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
       {
           // Call authService.LoginAsync
           // Return 401 on invalid credentials
       }

       [HttpPost("refresh")]
       public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
       {
           // Call authService.RefreshTokenAsync
           // Return 401 on invalid/expired refresh token
       }

       [HttpPost("logout")]
       public async Task<IActionResult> Logout()
       {
           // Invalidate refresh token
       }
   }
   ```

2. Add appropriate HTTP status codes:
   - 200 for successful login/register
   - 201 for successful registration (with Location header)
   - 400 for validation errors
   - 401 for authentication failures
   - 404 for user not found

**Success Criteria:**
- All 4 endpoints are accessible
- Proper error handling and status codes
- Input validation on all DTOs
- Controller integrates with AuthService

**Time Estimate:** 1.5 hours

---

### Task 1-E: Configure JWT Authentication in Program.cs

**Objective:** Set up JWT bearer authentication middleware and authorization policies

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Program.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/appsettings.json`

**Instructions:**

1. **Update `appsettings.json`:**
   ```json
   {
     "JwtSettings": {
       "SecretKey": "YOUR_SUPER_SECRET_KEY_CHANGE_IN_PRODUCTION_MIN_32_CHARS",
       "Issuer": "MoviesApi",
       "Audience": "MoviesAppUsers",
       "AccessTokenExpirationMinutes": 15,
       "RefreshTokenExpirationDays": 7
     },
     // ... existing config
   }
   ```

2. **Update `Program.cs`:**
   - Add `using Microsoft.AspNetCore.Authentication.JwtBearer;`
   - Add `using Microsoft.AspNetCore.Identity;`
   - Add `using Microsoft.IdentityModel.Tokens;`
   - Add `using System.Text;`

   **Before `var app = builder.Build();`:**
   ```csharp
   // Add Identity
   builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
   {
       options.Password.RequiredLength = 8;
       options.Password.RequireDigit = true;
       options.Password.RequireLowercase = true;
       options.Password.RequireUppercase = true;
       options.Password.RequireNonAlphanumeric = true;
       options.User.RequireUniqueEmail = true;
   })
   .AddEntityFrameworkStores<MoviesDbContext>()
   .AddDefaultTokenValidators();

   // Add JWT Authentication
   var jwtSettings = builder.Configuration.GetSection("JwtSettings");
   var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

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
   ```

   **After `var app = builder.Build();`:**
   ```csharp
   app.UseAuthentication();
   app.UseAuthorization();
   ```

3. **Configure CORS:**
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowFrontend", policy =>
       {
           policy.WithOrigins("http://localhost:4200", "https://your-production-domain.com")
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials();
       });
   });
   ```

   Use CORS policy: `app.UseCors("AllowFrontend");`

**Success Criteria:**
- JWT authentication is configured
- Authorization policies are defined
- CORS is restricted to specific origins
- Project builds and runs successfully

**Time Estimate:** 1 hour

---

## Phase 2: Authorization & Protection

### Task 2-A: Add Authorization to Existing Controllers

**Objective:** Protect all existing controller endpoints with authentication and authorization

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/MovieController.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/FilmGenresController.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/MediaTypesController.cs`

**Instructions:**

1. **Add using statements:**
   ```csharp
   using Microsoft.AspNetCore.Authorization;
   ```

2. **Add `[Authorize]` attribute to all controllers:**
   ```csharp
   [Authorize]
   [ApiController]
   [Route("api/v1/[controller]")]
   public class MovieController : ControllerBase
   ```

3. **Differentiate access levels:**
   - GET endpoints: `[Authorize(Policy = "UserOnly")]`
   - POST/PUT/DELETE endpoints: `[Authorize(Policy = "UserOnly")]` (or create "CanModifyMovies" policy)
   - Consider allowing GET without auth for public data (use `[AllowAnonymous]`)

4. **Update controller methods to get current user:**
   ```csharp
   private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
       ?? throw new UnauthorizedAccessException("User ID not found");
   ```

**Success Criteria:**
- All endpoints require authentication
- Unauthenticated requests return 401
- Unauthorized requests return 403
- Current user ID can be retrieved in controller methods

**Time Estimate:** 1 hour

---

### Task 2-B: Implement Ownership Validation

**Objective:** Ensure users can only access/modify their own resources

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Models/Movie.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Repositories/IMovieRepository.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Repositories/MovieRepository.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/MovieController.cs`

**Instructions:**

1. **Add `OwnerId` to `Movie` model:**
   ```csharp
   [Required]
   public string OwnerId { get; set; } = string.Empty;
   ```

2. **Update repository methods:**
   - Add `string userId` parameter to `GetByIdAsync`, `UpdateAsync`, `DeleteAsync`
   - Add ownership check: verify `movie.OwnerId == userId` before operations
   - Update `CreateAsync` to set `OwnerId` from parameter
   - Update `GetAllAsync` to filter by owner: `.Where(m => m.OwnerId == userId)`

3. **Update controller methods:**
   - Get user ID from claims: `var userId = GetUserId();`
   - Pass `userId` to repository methods
   - Return 403 Forbidden if user doesn't own the resource

4. **Create migration:**
   - Run `dotnet ef migrations add AddOwnerIdToMovies`
   - Run `dotnet ef database update`

**Success Criteria:**
- Movies are associated with owners
- Users can only see their own movies in GetAll
- Users cannot access/modify/delete others' movies
- 403 returned on ownership violations

**Time Estimate:** 2 hours

---

### Task 2-C: Create Separate DTOs for Create/Update/Read

**Objective:** Prevent unauthorized property modifications with role-specific DTOs

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/DTOs/MovieDtos.cs`

**Instructions:**

1. **Refactor existing DTOs:**
   ```csharp
   // For creating movies
   public class MovieCreateDto
   {
       [Required]
       [StringLength(200, MinimumLength = 1)]
       public string Title { get; set; } = string.Empty;
       
       [Required]
       [Range(1888, 2100)]
       public int ReleaseYear { get; set; }
       
       public List<int> MediaTypeIds { get; set; } = new();
       public List<int> FilmGenreIds { get; set; } = new();
   }

   // For updating movies (subset of properties)
   public class MovieUpdateDto
   {
       [StringLength(200, MinimumLength = 1)]
       public string? Title { get; set; }
       
       [Range(1888, 2100)]
       public int? ReleaseYear { get; set; }
       
       public List<int>? MediaTypeIds { get; set; }
       public List<int>? FilmGenreIds { get; set; }
   }

   // For responses (includes all data)
   public class MovieResponseDto
   {
       public int Id { get; set; }
       public string Title { get; set; } = string.Empty;
       public int ReleaseYear { get; set; }
       public string OwnerId { get; set; } = string.Empty;
       public DateTime CreatedAt { get; set; }
       public DateTime? UpdatedAt { get; set; }
       public List<MediaTypeDto> MediaTypes { get; set; } = new();
       public List<FilmGenreDto> FilmGenres { get; set; } = new();
   }
   ```

2. **Add validation attributes:**
   - `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, etc.
   - Use FluentValidation or DataAnnotations

3. **Update controllers to use new DTOs:**
   - POST: `[FromBody] MovieCreateDto`
   - PUT: `[FromBody] MovieUpdateDto`
   - GET: Return `MovieResponseDto`

**Success Criteria:**
- Separate DTOs for each operation type
- Validation attributes on all properties
- Controllers use appropriate DTOs
- Cannot update read-only properties (OwnerId, CreatedAt, etc.)

**Time Estimate:** 1.5 hours

---

### Task 2-D: Implement Global Exception Handling

**Objective:** Prevent sensitive information leakage through error messages

**Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Middleware/ExceptionHandlingMiddleware.cs`

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Program.cs`

**Instructions:**

1. **Create `ExceptionHandlingMiddleware.cs`:**
   ```csharp
   using System.Net;
   using System.Text.Json;

   namespace MoviesApi.Middleware;

   public class ExceptionHandlingMiddleware
   {
       private readonly RequestDelegate _next;
       private readonly ILogger<ExceptionHandlingMiddleware> _logger;

       public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
       {
           _next = next;
           _logger = logger;
       }

       public async Task InvokeAsync(HttpContext context)
       {
           try
           {
               await _next(context);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
               await HandleExceptionAsync(context, ex);
           }
       }

       private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
       {
           context.Response.ContentType = "application/json";
           
           var response = new
           {
               error = GetErrorMessage(exception),
               details = "An error occurred while processing your request",
               path = context.Request.Path
           };

           context.Response.StatusCode = exception switch
           {
               UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
               ArgumentException => StatusCodes.Status400BadRequest,
               KeyNotFoundException => StatusCodes.Status404NotFound,
               _ => StatusCodes.Status500InternalServerError
           };

           await context.Response.WriteAsync(JsonSerializer.Serialize(response));
       }

       private static string GetErrorMessage(Exception ex) => ex switch
       {
           UnauthorizedAccessException => "Unauthorized",
           ArgumentException => "Invalid request",
           KeyNotFoundException => "Resource not found",
           _ => "Internal server error"
       };
   }
   ```

2. **Register middleware in `Program.cs`:**
   - Add `using MoviesApi.Middleware;`
   - Add `app.UseMiddleware<ExceptionHandlingMiddleware>();` after `app.UseRouting()` but before `app.MapControllers()`

3. **Remove or update existing error handling:**
   - Ensure no detailed stack traces are returned to clients

**Success Criteria:**
- All exceptions caught by middleware
- Generic error messages returned to clients
- Detailed errors logged internally
- Appropriate HTTP status codes returned

**Time Estimate:** 1 hour

---

## Phase 3: Resource Protection

### Task 3-A: Implement Rate Limiting

**Objective:** Prevent API abuse with rate limiting middleware

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Program.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/appsettings.json`

**Instructions:**

1. **Add rate limiting configuration to `appsettings.json`:**
   ```json
   "RateLimiting": {
     "GlobalLimit": 100,
     "WriteLimit": 10,
     "WindowInSeconds": 60
   }
   ```

2. **Update `Program.cs`:**
   - Add `using System.Threading.RateLimiting;`
   - Add `builder.Services.AddRateLimiter(options =>
     {
         options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
             RateLimitPartition.GetFixedWindowLimiter(
                 partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
                 factory: partition => new FixedWindowRateLimiterOptions
                 {
                     PermitLimit = 100,
                     Window = TimeSpan.FromMinutes(1)
                 }));

         // Separate limiter for write operations
         options.AddPolicy("WriteLimit", httpContext =>
             RateLimitPartition.GetFixedWindowLimiter(
                 partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
                 factory: partition => new FixedWindowRateLimiterOptions
                 {
                     PermitLimit = 10,
                     Window = TimeSpan.FromMinutes(1)
                 }));

         options.OnRejected = async (context, token) =>
         {
             context.HttpContext.Response.StatusCode = 429;
             context.HttpContext.Response.Headers.Add("Retry-After", "60");
             await context.HttpContext.Response.WriteAsJsonAsync(new
             {
                 error = "Too many requests",
                 message = "Rate limit exceeded. Please try again later."
             }, token);
         };
     });`

3. **Apply rate limiting to controllers:**
   - Add `[EnableRateLimiting("WriteLimit")]` to POST/PUT/DELETE methods
   - Global limiter applies to all endpoints automatically

**Success Criteria:**
- Rate limiting is active (100 req/min global, 10 req/min for writes)
- 429 status returned when limit exceeded
- `Retry-After` header included in response
- Rate limits configurable via appsettings

**Time Estimate:** 1.5 hours

---

### Task 3-B: Implement Pagination

**Objective:** Prevent resource exhaustion from large data queries

**Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/DTOs/PaginationDtos.cs`

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/MovieController.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/FilmGenresController.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/MediaTypesController.cs`

**Instructions:**

1. **Create `PaginationDtos.cs`:**
   ```csharp
   namespace MoviesApi.DTOs;

   public class PagedResponse<T>
   {
       public List<T> Items { get; set; } = new();
       public int PageNumber { get; set; }
       public int PageSize { get; set; }
       public int TotalCount { get; set; }
       public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
       public bool HasPrevious => PageNumber > 1;
       public bool HasNext => PageNumber < TotalPages;
   }

   public class PaginationParameters
   {
       private const int MaxPageSize = 100;
       private int _pageSize = 20;

       public int PageNumber { get; set; } = 1;

       public int PageSize
       {
           get => _pageSize;
           set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
       }
   }
   ```

2. **Update repository methods:**
   - Add pagination parameters to `GetAllAsync(int pageNumber, int pageSize)`
   - Use `.Skip((pageNumber - 1) * pageSize).Take(pageSize)`
   - Return total count along with items (or use `IQueryable`)

3. **Update controllers:**
   - Add `[FromQuery] PaginationParameters pagination` parameter to GET methods
   - Return `PagedResponse<T>` instead of `List<T>`
   - Validate pagination parameters (1 <= page, 1 <= pageSize <= 100)

**Success Criteria:**
- All list endpoints support pagination
- Default page size: 20, Max page size: 100
- Response includes pagination metadata (total, page, pages)
- Invalid pagination returns 400 Bad Request

**Time Estimate:** 2 hours

---

### Task 3-C: Add Security Headers Middleware

**Objective:** Protect against common web vulnerabilities with security headers

**Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Middleware/SecurityHeadersMiddleware.cs`

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Program.cs`

**Instructions:**

1. **Create `SecurityHeadersMiddleware.cs`:**
   ```csharp
   namespace MoviesApi.Middleware;

   public class SecurityHeadersMiddleware
   {
       private readonly RequestDelegate _next;

       public SecurityHeadersMiddleware(RequestDelegate next)
       {
           _next = next;
       }

       public async Task InvokeAsync(HttpContext context)
       {
           context.Response.OnStarting(() =>
           {
               // Prevent MIME type sniffing
               context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
               
               // Prevent clickjacking
               context.Response.Headers.Add("X-Frame-Options", "DENY");
               
               // XSS protection (legacy but still useful)
               context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
               
               // Referrer policy
               context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
               
               // Permissions policy (formerly Feature-Policy)
               context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
               
               // Content Security Policy
               context.Response.Headers.Add("Content-Security-Policy", 
                   "default-src 'self'; object-src 'none'; frame-ancestors 'none';");

               // HSTS (only in production)
               if (!context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
               {
                   context.Response.Headers.Add("Strict-Transport-Security", 
                       "max-age=31536000; includeSubDomains; preload");
               }

               return Task.CompletedTask;
           });

           await _next(context);
       }
   }
   ```

2. **Register middleware in `Program.cs`:**
   - Add `app.UseMiddleware<SecurityHeadersMiddleware>();` early in the pipeline (after `app.UseRouting()`)

3. **Test headers:**
   - Run API and check response headers with browser dev tools or curl

**Success Criteria:**
- All security headers present in responses
- HSTS only in production
- CSP prevents inline scripts and external resources
- Headers pass security scanner (e.g., securityheaders.com)

**Time Estimate:** 1 hour

---

### Task 3-D: Implement Audit Logging

**Objective:** Track all sensitive operations for security monitoring

**Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Models/AuditLog.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Services/AuditLogService.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Middleware/AuditLoggingMiddleware.cs`

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Data/MoviesDbContext.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Program.cs`

**Instructions:**

1. **Create `AuditLog.cs` model:**
   ```csharp
   using System.ComponentModel.DataAnnotations;

   namespace MoviesApi.Models;

   public class AuditLog
   {
       [Key]
       public int Id { get; set; }
       public DateTime Timestamp { get; set; } = DateTime.UtcNow;
       public string UserId { get; set; } = string.Empty;
       public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
       public string EntityType { get; set; } = string.Empty; // Movie, User, etc.
       public string? EntityId { get; set; }
       public string? OldValues { get; set; } // JSON
       public string? NewValues { get; set; } // JSON
       public string IpAddress { get; set; } = string.Empty;
       public string UserAgent { get; set; } = string.Empty;
   }
   ```

2. **Add `DbSet<AuditLog>` to `MoviesDbContext.cs`**

3. **Create `AuditLogService.cs`:**
   - Method: `LogAsync(string userId, string action, string entityType, string? entityId, object? oldValues, object? newValues, string ipAddress, string userAgent)`
   - Serialize objects to JSON
   - Save to database (fire-and-forget or use background queue)

4. **Create `AuditLoggingMiddleware.cs`:**
   - Capture request details (method, path, user, IP, user-agent)
   - Log all POST/PUT/DELETE requests
   - Optionally capture request/response bodies

5. **Register in `Program.cs`:**
   - Add service: `builder.Services.AddScoped<AuditLogService>();`
   - Add middleware: `app.UseMiddleware<AuditLoggingMiddleware>();`

**Success Criteria:**
- All mutations (CREATE/UPDATE/DELETE) are logged
- Logs include user ID, timestamp, action, entity details
- Logs are queryable via database
- Performance impact is minimal

**Time Estimate:** 2 hours

---

## Phase 4: Hardening

### Task 4-A: Migrate from Integer IDs to GUIDs

**Objective:** Prevent ID enumeration attacks by using non-sequential identifiers

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Models/Movie.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/DTOs/MovieDtos.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Controllers/MovieController.cs`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Repositories/MovieRepository.cs`

**Instructions:**

1. **Update `Movie.cs`:**
   ```csharp
   [Key]
   public Guid Id { get; set; } = Guid.NewGuid();
   ```

2. **Update DTOs:**
   - Change `int Id` to `Guid Id` in all DTOs

3. **Update controllers:**
   - Change route parameters: `[HttpGet("{id:guid}")]`
   - Update method signatures: `GetById(Guid id)`

4. **Update repository:**
   - Change all `int id` parameters to `Guid id`
   - Update LINQ queries to use Guid

5. **Create and apply migration:**
   ```bash
   dotnet ef migrations add ChangeMovieIdToGuid
   dotnet ef database update
   ```

6. **Update other entities (optional but recommended):**
   - Apply same pattern to `MediaType`, `FilmGenre`, etc.

**Success Criteria:**
- All Movie IDs are GUIDs
- API endpoints accept GUIDs
- Database migration applied successfully
- Existing data migrated (or cleared for development)

**Time Estimate:** 2 hours

---

### Task 4-B: Implement API Versioning

**Objective:** Enable safe API evolution with versioning

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Program.cs`
- All controllers

**Instructions:**

1. **Update `Program.cs`:**
   - Add `using Microsoft.AspNetCore.Mvc;`
   - Add versioning: 
   ```csharp
   builder.Services.AddApiVersioning(options =>
   {
       options.DefaultApiVersion = new ApiVersion(1, 0);
       options.AssumeDefaultVersionWhenUnspecified = true;
       options.ReportApiVersions = true;
       options.ApiVersionReader = ApiVersionReader.Combine(
           new UrlSegmentApiVersionReader(),
           new HeaderApiVersionReader("X-API-Version"),
           new QueryStringApiVersionReader("api-version")
       );
   });

   builder.Services.AddVersionedApiExplorer(options =>
   {
       options.GroupNameFormat = "'v'VVV";
       options.SubstituteApiVersionInUrl = true;
   });
   ```

2. **Update controllers:**
   - Add `[ApiVersion("1.0")]` to all controllers
   - Update route: `[Route("api/v{version:apiVersion}/[controller]")]`

3. **Update Swagger configuration:**
   - Install `Microsoft.AspNetCore.OpenApi` if needed
   - Configure Swagger to show API versions

**Success Criteria:**
- API versioning is active
- Version can be specified via URL, header, or query string
- Swagger shows versioned endpoints
- Default version is 1.0

**Time Estimate:** 1.5 hours

---

### Task 4-C: Secure Configuration Management

**Objective:** Move sensitive configuration out of appsettings.json

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/appsettings.json`
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Program.cs`

**Files to Create:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Properties/launchSettings.json` (update)

**Instructions:**

1. **Remove sensitive data from `appsettings.json`:**
   - Remove `JwtSettings.SecretKey` or replace with placeholder
   - Remove connection strings or use placeholder

2. **Use User Secrets for development:**
   ```bash
   cd /home/user/src/MoviesApp/apps/web-api/MoviesApi
   dotnet user-secrets init
   dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-key-min-32-chars-here"
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=movies.db"
   ```

3. **Update `Program.cs` to read from environment:**
   ```csharp
   var secretKey = builder.Configuration["JwtSettings:SecretKey"] 
       ?? builder.Configuration["JWT_SECRET_KEY"] // Environment variable
       ?? throw new InvalidOperationException("JWT SecretKey not configured");
   ```

4. **Document required environment variables:**
   - Create `.env.example` file with placeholder values
   - List all required secrets

**Success Criteria:**
- No secrets in source control
- Secrets loaded from User Secrets (dev) or environment variables (prod)
- Application fails gracefully if secrets are missing
- Documentation provided for required variables

**Time Estimate:** 1 hour

---

### Task 4-D: Protect Swagger in Production

**Objective:** Prevent unauthorized access to API documentation

**Files to Modify:**
- `/home/user/src/MoviesApp/apps/web-api/MoviesApi/Program.cs`

**Instructions:**

**Option 1: Disable Swagger in Production (Recommended)**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**Option 2: Protect Swagger with Authentication**
```csharp
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwaggerInProduction"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger";
        // Add basic auth or JWT requirement
    });
}
```

**Option 3: Use a separate endpoint with authorization**
- Create `/admin/swagger` endpoint with `[Authorize(Policy = "AdminOnly")]`

**Recommendation:** Use Option 1 for simplicity. Swagger should only be available in development environments.

**Success Criteria:**
- Swagger not accessible in production by default
- If enabled, requires authentication
- No sensitive information exposed in Swagger docs (use `[ApiExplorerSettings(IgnoreApi = true)]` for sensitive endpoints)

**Time Estimate:** 30 minutes

---

## Testing & Validation

### Task 5-A: Security Testing Checklist

**Objective:** Validate all security implementations

**Instructions:**

Run through this checklist after all phases are complete:

**Authentication Tests:**
- [ ] Register a new user - verify 201 Created
- [ ] Login with valid credentials - verify JWT token received
- [ ] Login with invalid credentials - verify 401 Unauthorized
- [ ] Access protected endpoint without token - verify 401
- [ ] Access protected endpoint with valid token - verify 200 OK
- [ ] Refresh token flow - verify new tokens issued
- [ ] Expired token - verify 401

**Authorization Tests:**
- [ ] User A tries to access User B's movie - verify 403 Forbidden
- [ ] User A tries to update User B's movie - verify 403
- [ ] Unauthenticated user tries to create movie - verify 401

**Rate Limiting Tests:**
- [ ] Send 100+ requests in 1 minute - verify 429 after limit
- [ ] Send 10+ write requests in 1 minute - verify 429
- [ ] Verify `Retry-After` header in 429 response

**Input Validation Tests:**
- [ ] Submit movie with empty title - verify 400
- [ ] Submit movie with title > 200 chars - verify 400
- [ ] Submit movie with release year < 1888 or > 2100 - verify 400
- [ ] Submit invalid pagination parameters - verify 400

**Security Headers Tests:**
- [ ] Check response headers with curl or browser dev tools
- [ ] Verify X-Content-Type-Options: nosniff
- [ ] Verify X-Frame-Options: DENY
- [ ] Verify Content-Security-Policy present
- [ ] Verify Strict-Transport-Security (in production)

**Error Handling Tests:**
- [ ] Trigger an exception - verify generic error message
- [ ] Verify no stack traces in response
- [ ] Verify detailed error logged internally

**Audit Logging Tests:**
- [ ] Create a movie - verify audit log entry
- [ ] Update a movie - verify audit log with old/new values
- [ ] Delete a movie - verify audit log entry

**Tools:**
- Use Postman or curl for API testing
- Use OWASP ZAP for automated security scanning
- Use browser dev tools for header inspection

**Time Estimate:** 3 hours

---

## Execution Order

**Recommended sequence for sub-agents:**

1. **Task 1-A** (Install Packages) - 15 min
2. **Task 1-B** (User Model) - 30 min
3. **Task 1-C** (Auth Service) - 2 hours
4. **Task 1-D** (Auth Controller) - 1.5 hours
5. **Task 1-E** (JWT Config) - 1 hour
6. **Task 2-A** (Add Authorization) - 1 hour
7. **Task 2-B** (Ownership Validation) - 2 hours
8. **Task 2-C** (Separate DTOs) - 1.5 hours
9. **Task 2-D** (Exception Handling) - 1 hour
10. **Task 3-A** (Rate Limiting) - 1.5 hours
11. **Task 3-B** (Pagination) - 2 hours
12. **Task 3-C** (Security Headers) - 1 hour
13. **Task 3-D** (Audit Logging) - 2 hours
14. **Task 4-A** (GUID Migration) - 2 hours
15. **Task 4-B** (API Versioning) - 1.5 hours
16. **Task 4-C** (Secure Config) - 1 hour
17. **Task 4-D** (Protect Swagger) - 30 min
18. **Task 5-A** (Security Testing) - 3 hours

**Total:** ~26 hours (can be parallelized)

**Parallel Execution:**
- Tasks 1-A through 1-E must be sequential (Phase 1 foundation)
- Tasks 2-A through 2-D can run in parallel after Phase 1
- Tasks 3-A through 3-D can run in parallel after Phase 2
- Tasks 4-A through 4-D can run in parallel after Phase 3
- Task 5-A runs last (validation)

---

## Notes for Sub-Agent Execution

1. **Always build after changes:** `dotnet build`
2. **Run tests if they exist:** `dotnet test`
3. **Create migrations for model changes:** `dotnet ef migrations add <Name>`
4. **Apply migrations:** `dotnet ef database update`
5. **Test endpoints manually** with curl or Postman
6. **Document any deviations** from the plan
7. **Ask for clarification** if requirements are unclear

---

**Document Version:** 1.0  
**Created:** 2026-05-18  
**For Use With:** GitHub Copilot Sub-Agents

# Copilot Instructions for MoviesWeb

## Project Overview
MoviesWeb is an ASP.NET Core 10.0 web application demonstrating MVC/Razor Pages architecture. It fetches movie data from an external HTTP API via dependency-injected services and displays it through controllers and views.

## Architecture & Key Concepts

### Layered Architecture
1. **Controllers** (`Controllers/*.cs`) - HTTP request handlers that delegate business logic to services
   - `MoviesController` - Movie browsing and management operations
   - `HomeController` - Landing page
   - Use dependency injection to consume services; never make direct HTTP calls

2. **Services** (`Services/*.cs`) - Encapsulate HTTP communication and data operations
   - `IMoviesService` / `MoviesService` - Interfaces and implementations for movie data access
   - Always use async patterns (`Task<T>`)
   - HttpClient is injected via DI (configured in `Program.cs`)
   - External API base URL comes from config: `builder.Configuration["MoviesApi:BaseUrl"]`

3. **Models** (`Models/*.cs`) - Domain objects and interfaces
   - Implement corresponding interfaces (`IMovie`, `IUser`, etc.)
   - Include XML doc comments on public members
   - Example: `Movie` class maps JSON responses from the movies API

4. **Views** (`Views/*.cshtml`) - Razor views and pages for rendering HTML
   - Strict separation: views receive data from controllers, never make HTTP calls
   - Use `_Layout.cshtml` for shared UI structure

### Data Flow Pattern
```
HTTP Request → Controller.Action() 
  → IMoviesService.Method() 
    → HttpClient.GetFromJsonAsync<T>()/"Movie API" 
      → Service returns Movie/IEnumerable<Movie> 
        → Controller View(movieData) 
          → Rendered HTML Response
```

## Code Conventions

### Namespaces & Imports
- **Global usings enabled**: `GlobalUsings.cs` provides common ASP.NET/System/MoviesWeb imports (do not duplicate these in new code)
- Services use file-scoped namespaces: `namespace MoviesWeb.Services;`
- Controllers use block namespaces: `namespace MyApp.Namespace { ... }`
- New files should follow the file-scoped pattern unless maintaining consistency with existing block-style files

### Naming & Style
- **PascalCase** for classes, interfaces, properties, public methods
- **camelCase** for local variables and parameters
- **Prefixed with underscore** for private fields: `private readonly HttpClient _httpClient;`
- **Interface names start with 'I'**: `IMoviesService`, `IMovie`
- **Nullable reference types enabled** (`<Nullable>enable</Nullable>`): explicit null checks required

### Error Handling Patterns
Services throw `HttpRequestException` on API failures with descriptive messages:
```csharp
if (!response.IsSuccessStatusCode) {
    throw new HttpRequestException($"Failed to ... Status code: {response.StatusCode}");
}
```

## Dependency Injection & Configuration

### Service Registration (Program.cs)
Services are registered in `Program.cs` using `builder.Services`:
```csharp
builder.Services.AddHttpClient<IMoviesService, MoviesService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MoviesApi:BaseUrl"] ?? string.Empty);
});
```

**Never instantiate services directly** — inject them via constructor parameters.

### Configuration
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- Access via `IConfiguration` in DI or `builder.Configuration` during setup
- API endpoint: `MoviesApi:BaseUrl` config key

## Development Workflow

### Build & Run
```bash
# Build
dotnet build MoviesWeb/MoviesWeb.csproj

# Run (uses Development settings by default)
dotnet run --project MoviesWeb/MoviesWeb.csproj

# Publish Release build
dotnet publish MoviesWeb/MoviesWeb.csproj -c Release -o out
```

### Target Framework
- **.NET 10.0** - specified in `MoviesWeb.csproj`
- Ensure your SDK version matches (`dotnet --version`)

### Project File
- Location: `MoviesWeb/MoviesWeb.csproj`
- Key settings: `<Nullable>enable</Nullable>`, `<ImplicitUsings>disable</ImplicitUsings>`
- No additional NuGet packages in current implementation (uses base ASP.NET Core libraries)

## Common Tasks & Patterns

### Adding a New Movie API Endpoint
1. Add method to `IMoviesService` with XML docs and async signature
2. Implement in `MoviesService` using `_httpClient.GetFromJsonAsync<T>()` or appropriate method
3. Add controller action in `MoviesController` that calls service and returns `View(result)`
4. Create Razor view in `Views/Movies/` to render the model

### Querying Movie Data
- Use `MovieQuery` class to build dynamic queries; implement `.Validate()` before use
- Service method: `GetMovieByQueryAsync(MovieQuery query)` - passes as query string params

### Modifying Models
- Update both the concrete class (e.g., `Movie`) and corresponding interface (e.g., `IMovie`)
- Ensure JSON deserialization aligns with API response structure (properties must match JSON field names or use `[JsonPropertyName]` attributes if needed)

## File Organization Reference
- **Controllers/** - HTTP handlers only (thin, stateless)
- **Services/** - All external API communication and business logic
- **Models/** - Domain entities and query objects
- **Views/** - Razor templates (cshtml)
- **ViewModels/** - View-specific data transfer objects (if needed for complex views)
- **wwwroot/** - Static assets (CSS, JS, libraries)

## Testing & Future Considerations
- No unit tests currently in this demonstration project
- To add tests: create parallel `MoviesWeb.Tests` project with `xUnit` or `NUnit`
- Mock `IMoviesService` in controller tests using frameworks like `Moq`

## Quick Debugging Tips
- **Startup issues**: Verify `appsettings.json` has valid `MoviesApi:BaseUrl`
- **API failures**: Add logging to `MoviesService` methods to inspect HTTP responses
- **Null reference exceptions**: Check `?? new Movie()` fallback patterns in service methods
- **404 errors in controllers**: Ensure view path matches `Controller/Action` convention (e.g., `Views/Movies/Index.cshtml`)

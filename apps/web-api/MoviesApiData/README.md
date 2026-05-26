# MoviesApiData

**Data Access Layer** - Entity Framework Core DbContext, repositories, and data persistence.

## Purpose
Encapsulates all data access logic including the Entity Framework Core DbContext, repository implementations, and database migrations.

## Contents
- `Data/`
  - `MoviesDbContext.cs` - EF Core DbContext with all entity configurations
- `Migrations/` - Database migration files
- `Repositories/` - Repository pattern implementations:
  - `IMovieRepository.cs` / `MovieRepository.cs`
  - `IMediaTypeRepository.cs` / `MediaTypeRepository.cs`
  - `IFilmGenreRepository.cs` / `FilmGenreRepository.cs`
- `movies.db` - SQLite database file

## Dependencies
- .NET 10.0
- `MoviesApiDomain` - References domain entities
- `Microsoft.EntityFrameworkCore` (10.0.x)
- `Microsoft.EntityFrameworkCore.Sqlite` (10.0.x)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.x)

## Architecture
This layer sits between the domain layer and the API layer:
- **References**: MoviesApiDomain (for entities)
- **Referenced by**: MoviesApi (for data access)
- **No upward dependencies**: Never references MoviesApi

## Repository Pattern
Implements the repository pattern for data access:

```csharp
// Usage example
public class MovieService
{
    private readonly IMovieRepository _repository;
    
    public async Task<Movie> GetMovieAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
```

## EF Core Configuration
All entity configurations are in `MoviesDbContext.OnModelCreating()` using Fluent API. This keeps entity classes clean and separates data access concerns.

## Migrations
Manage database schema changes:

```bash
# Add new migration
dotnet ef migrations add MigrationName \
  --project MoviesApiData \
  --startup-project MoviesApi

# Update database
dotnet ef database update \
  --project MoviesApiData \
  --startup-project MoviesApi

# Remove last migration
dotnet ef migrations remove \
  --project MoviesApiData \
  --startup-project MoviesApi
```

## Database
Uses SQLite for development. Connection string configured in `MoviesApi/appsettings.json`.

## Related Projects
- **MoviesApiDomain** - Referenced for entity definitions
- **MoviesApi** - References this for data access
- **MoviesApi.Tests** - References this for integration tests

# MoviesApiDomain

**Domain Layer** - Pure business entities with no external framework dependencies.

## Purpose
Contains the core business entities (domain models) for the Movies application. This layer has minimal dependencies and focuses on representing the business domain.

## Contents
- `Models/` - Domain entities:
  - `Movie.cs` - Movie entity
  - `MediaType.cs` - Media format types (DVD, Blu-Ray, Digital, etc.)
  - `FilmGenre.cs` - Film genre categories
  - `MovieMediaType.cs` - Junction entity for Movie-MediaType relationship
  - `MovieFilmGenre.cs` - Junction entity for Movie-Genre relationship
  - `ApplicationUser.cs` - Identity user with extended properties
  - `RefreshToken.cs` - JWT refresh token entity
  - `AuditLog.cs` - Audit trail entity

## Dependencies
- .NET 10.0
- `System.ComponentModel.Annotations` - For data validation attributes
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` - For Identity base classes

## Architecture
This is the innermost layer with **no dependencies** on other projects in the solution. It can be referenced by any layer without creating circular dependencies.

## Key Principles
- **POCOs**: Entities are Plain Old CLR Objects
- **Data Annotations**: Uses `[Required]`, `[StringLength]`, etc. for validation
- **No EF Core Dependencies**: No DbContext or Fluent API configurations here
- **Business Logic Free**: Entities are data containers, not business logic holders

## Usage
```csharp
using MoviesApiDomain.Models;

var movie = new Movie
{
    Title = "Inception",
    ReleaseYear = 2010,
    OwnerId = userId
};
```

## Related Projects
- **MoviesApiData** - References this project for entity definitions
- **MoviesApi** - References this project for domain models

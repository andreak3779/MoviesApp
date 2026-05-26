# MoviesApi

**Web API Layer** - ASP.NET Core Web API for the Movies application.

## Purpose
Provides RESTful API endpoints for managing movie collections. This is the presentation layer that handles HTTP requests, authentication, and business logic orchestration.

## Project Structure

### Core Files
- `Program.cs` - Application entry point, dependency injection, middleware configuration
- `MoviesApi.csproj` - Project file with package and project references
- `appsettings.json` - Application configuration and connection strings
- `Properties/launchSettings.json` - Development launch profiles

### Folders
- `Controllers/` - API controllers:
  - `MovieController.cs` - CRUD operations for movies
  - `AuthController.cs` - Authentication and authorization
  - `FilmGenresController.cs` - Genre management
  - `MediaTypesController.cs` - Media type management
- `Services/` - Business logic services:
  - `AuthService.cs` - JWT authentication, registration, login
  - `AuditLogService.cs` - Audit trail logging
- `DTOs/` - Data Transfer Objects:
  - `AuthDtos.cs` - Authentication request/response DTOs
  - `MovieDtos.cs` - Movie CRUD DTOs
  - `PaginationDtos.cs` - Pagination support DTOs
- `Middleware/` - Custom middleware:
  - `ExceptionHandlingMiddleware.cs` - Global error handling
  - `RateLimitingMiddleware.cs` - API rate limiting
  - `SecurityHeadersMiddleware.cs` - Security header injection
  - `AuditLoggingMiddleware.cs` - Request audit logging

## Dependencies
- .NET 10.0
- `MoviesApiDomain` - Domain entities
- `MoviesApiData` - Data access layer
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `Asp.Versioning.Mvc` - API versioning
- `AutoMapper` - Object mapping
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI documentation

## Architecture
This is the outermost layer following Clean Architecture principles:

```
MoviesApi (Web API)
    ↓ references
MoviesApiData (Data Access)
    ↓ references
MoviesApiDomain (Domain Entities)
```

**Key Principles:**
- **Dependency Inversion**: Depends on abstractions (interfaces), not implementations
- **Separation of Concerns**: Controllers handle HTTP, Services handle business logic
- **DTO Pattern**: Never expose domain entities directly to API consumers

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- SQLite (for development database)

### Run the Application

```bash
# Restore dependencies
dotnet restore

# Run with watch mode (auto-reload on changes)
dotnet watch run --project MoviesApi

# Or run normally
dotnet run --project MoviesApi
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### API Documentation
Swagger UI is available at: `/swagger`

### Authentication
Most endpoints require JWT authentication. Register a user first:

```bash
POST /api/v1/Auth/register
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "firstName": "John",
  "lastName": "Doe"
}
```

Then login to get a JWT token:

```bash
POST /api/v1/Auth/login
{
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

Include the token in subsequent requests:
```
Authorization: Bearer <your-token>
```

## Testing

```bash
# Run unit tests
dotnet test MoviesApi.Tests/MoviesApi.Tests.csproj

# Run with coverage
dotnet test MoviesApi.Tests/MoviesApi.Tests.csproj --collect:"XPlat Code Coverage"
```

## Configuration

### Connection Strings
Configure in `appsettings.json` or use environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=movies.db"
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Development, Staging, Production
- `JWT_SECRET` - Secret key for JWT token generation
- `JWT_ISSUER` - Token issuer
- `JWT_AUDIENCE` - Token audience

## Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName \
  --project MoviesApiData \
  --startup-project MoviesApi

# Apply migrations
dotnet ef database update \
  --project MoviesApiData \
  --startup-project MoviesApi
```

## Related Projects
- **MoviesApiDomain** - Domain entities referenced by this project
- **MoviesApiData** - Data access layer referenced by this project
- **MoviesApi.Tests** - Unit and integration tests

## API Versioning
Uses URL path versioning: `/api/v1/movies`, `/api/v2/movies`, etc.

## Security Features
- JWT Bearer authentication
- Role-based authorization (Admin, User)
- Rate limiting
- Security headers (CORS, CSP, X-Frame-Options)
- Input validation
- Audit logging

4. The API will be available at `http://localhost:5000` (or the specified port).

## API Endpoints

The following endpoints are available:

- `GET /movies` - Retrieve a list of all movies.
- `GET /movies/{id}` - Retrieve a specific movie by ID.
- `POST /movies` - Add a new movie to the collection.
- `PUT /movies/{id}` - Update an existing movie.
- `DELETE /movies/{id}` - Remove a movie from the collection.

### Example Movie Object

```json
{
  "id": 1,
  "title": "Inception",
  "genre": "Sci-Fi",
  "releaseYear": 2010
}
```

## Running Tests

To run the test suite for this project, execute:

```
dotnet test
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

## License

This project is licensed under the MIT License. See the LICENSE file for details.
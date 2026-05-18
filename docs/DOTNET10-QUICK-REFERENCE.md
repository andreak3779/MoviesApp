# .NET 10 Quick Reference Guide

## Quick Start

### Build All Projects
```bash
# Web API
cd apps/web-api
dotnet build MoviesAPI.sln

# MVC Web
cd apps/mvc-web
dotnet build MoviesWeb.sln
```

### Run All Tests
```bash
# Web API Tests
cd apps/web-api
dotnet test MoviesApi.Tests/MoviesApi.Tests.csproj

# MVC Web Tests
cd apps/mvc-web
dotnet test MoviesWeb.Tests/MoviesWeb.Tests.csproj
```

### Run with Docker
```bash
cd /home/user/src/MoviesApp
docker-compose up -d
```

### Access Services
- **Web API**: http://localhost:8081
- **Swagger UI**: http://localhost:8081/swagger
- **MVC Web**: http://localhost:8080
- **Angular Client**: http://localhost:4000

---

## .NET 10 Highlights

### What's New
- **Improved Performance**: Faster JIT compilation and optimized runtime
- **Enhanced Type Safety**: Better nullable reference type support
- **EF Core 10**: Improved query performance and new features
- **Long-Term Support**: .NET 10 is an LTS version

### Migration Changes
- Target framework: `net10.0`
- EF Core packages: `10.*`
- ASP.NET Core packages: `10.*`
- Swagger/OpenAPI: Swashbuckle `7.*`

---

## Common Commands

### Development
```bash
# Run Web API (development)
cd apps/web-api/MoviesApi
dotnet run

# Run MVC Web (development)
cd apps/mvc-web/MoviesWeb
dotnet run

# Watch for changes
dotnet watch run
```

### Building
```bash
# Debug build
dotnet build -c Debug

# Release build
dotnet build -c Release

# No restore (faster)
dotnet build --no-restore
```

### Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"

# Verbose output
dotnet test --verbosity normal
```

### Publishing
```bash
# Publish Web API
dotnet publish MoviesApi/MoviesApi.csproj -c Release -o ./publish

# Publish MVC Web
dotnet publish MoviesWeb/MoviesWeb.csproj -c Release -o ./publish
```

### EF Core Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove migration
dotnet ef migrations remove

# List migrations
dotnet ef migrations list
```

---

## Package Management

### Update Packages
```bash
# Update all packages (interactive)
dotnet list package --outdated

# Update specific package
dotnet add package PackageName --version 10.*

# Remove package
dotnet remove package PackageName
```

### Current Package Versions (.NET 10)
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="10.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.*" />
```

---

## Docker Commands

### Build Images
```bash
# Build all images
docker-compose build

# Build specific service
docker-compose build web-api
```

### Run Containers
```bash
# Start all services
docker-compose up -d

# Start specific service
docker-compose up -d web-api

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f web-api
```

### Stop & Clean
```bash
# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Remove orphan containers
docker-compose down --remove-orphans
```

---

## Troubleshooting

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build

# Force restore
dotnet restore --force
```

### Docker Issues
```bash
# Remove all containers
docker rm -f $(docker ps -aq)

# Remove all images
docker rmi -f $(docker images -q)

# Prune system
docker system prune -a
```

### Port Conflicts
```bash
# Check port usage
lsof -i :8080
lsof -i :8081

# Kill process (Linux/Mac)
kill -9 <PID>

# Kill process (Windows)
taskkill /PID <PID> /F
```

### Database Issues
```bash
# Delete SQLite database
rm apps/web-api/MoviesApi/movies.db

# Recreate database
dotnet ef database update
```

---

## Development Tips

### Visual Studio Code Extensions
- C# Dev Kit (Microsoft)
- .NET Install Tool (Microsoft)
- Docker (Microsoft)
- EF Core Power Tools (optional)

### Recommended Settings
```json
{
  "dotnet.defaultSolution": "apps/web-api/MoviesAPI.sln",
  "dotnet.enableImportCompletion": true,
  "dotnet.showAllNotifications": true
}
```

### Debugging
```bash
# Launch with debug
dotnet run --launch-profile "Web API"

# Attach debugger
# Use VS Code "Attach to Process" command
```

---

## API Endpoints

### Web API (port 8081)
- `GET /health` - Health check
- `GET /api/movies` - List all movies
- `GET /api/movies/{id}` - Get movie by ID
- `POST /api/movies` - Create movie
- `PUT /api/movies/{id}` - Update movie
- `DELETE /api/movies/{id}` - Delete movie
- `GET /swagger` - Swagger UI

### MVC Web (port 8080)
- `GET /` - Home page
- `GET /Movies` - Movies list
- `GET /Movies/{id}` - Movie details
- `GET /Movies/Create` - Create form
- `POST /Movies` - Create submit
- `GET /Movies/Edit/{id}` - Edit form
- `POST /Movies/Edit/{id}` - Edit submit
- `GET /Movies/Delete/{id}` - Delete confirmation
- `POST /Movies/Delete/{id}` - Delete submit
- `GET /health` - Health check

---

## Environment Variables

### Web API
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
EnableHttpsRedirection=false
ConnectionStrings__DefaultConnection=Data Source=movies.db
```

### MVC Web
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
MoviesApi__BaseUrl=http://localhost:8081/api/
```

---

## Performance Tips

### Build Optimization
```bash
# Use Release configuration for performance builds
dotnet build -c Release

# Disable incremental builds for clean builds
dotnet build --no-incremental

# Parallelize builds
dotnet build --maxCpuCount
```

### Runtime Optimization
```bash
# Use ready-to-run compilation
dotnet publish -c Release -r linux-x64 --self-contained

# Use tiered compilation (default in .NET 10)
# No action needed - enabled by default
```

---

## Security Notes

### HTTPS Configuration
```bash
# Development certificate
dotnet dev-certs https --trust

# Export certificate
dotnet dev-certs https --export-path cert.pfx
```

### Secrets Management
```bash
# Set user secret
dotnet user-secrets set "KeyName" "Value"

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove KeyName

# Clear all secrets
dotnet user-secrets clear
```

---

## Getting Help

### Documentation
- [.NET 10 Documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-10)
- [ASP.NET Core 10](https://docs.microsoft.com/aspnet/core/)
- [EF Core 10](https://docs.microsoft.com/ef/core/)
- [Migration Summary](docs/DOTNET10-MIGRATION-SUMMARY.md)

### Community
- [Stack Overflow - .NET](https://stackoverflow.com/questions/tagged/.net)
- [.NET GitHub Discussions](https://github.com/dotnet/core/discussions)
- [ASP.NET Core GitHub](https://github.com/dotnet/aspnetcore)

---

## Version Information

**Current Versions:**
- .NET SDK: 10.0.107
- ASP.NET Core: 10.*
- EF Core: 10.*
- C#: 10 (default with .NET 10)

**Last Updated:** May 18, 2026

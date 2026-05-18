# Changelog

All notable changes to the MoviesApp project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Updated entire application stack to .NET 10, ASP.NET Core 10, and EF Core 10
- Migrated MoviesApi from .NET 9 to .NET 10
- Updated all NuGet packages to .NET 10 compatible versions

### Fixed
- Fixed nullability warnings in MovieRepository and MovieController
- Corrected Dockerfile project paths from MovieCollectionApi to MoviesApi
- Updated Docker runtime images to .NET 10

### Added
- .NET 10 migration documentation
- Comprehensive migration testing results

---

## [2.0.0] - 2026-05-18

### Changed
- **BREAKING**: Migrated from .NET 9 to .NET 10
- **BREAKING**: Migrated from EF Core 9 to EF Core 10
- **BREAKING**: Migrated from ASP.NET Core 9 to ASP.NET Core 10
- Updated target framework from `net9.0;net8.0` to `net10.0`
- Updated Docker base images to .NET 10

### Updated Dependencies
- Microsoft.EntityFrameworkCore: 9.* → 10.*
- Microsoft.EntityFrameworkCore.Design: 9.* → 10.*
- Microsoft.EntityFrameworkCore.Sqlite: 9.* → 10.*
- Microsoft.EntityFrameworkCore.Tools: 9.* → 10.*
- Microsoft.AspNetCore.Mvc.NewtonsoftJson: 6.0.0 → 10.*
- Swashbuckle.AspNetCore: 6.0.0 → 7.*
- Microsoft.NET.Test.Sdk: 17.10.0 → 17.12.*
- xunit: 2.8.1 → 2.9.*
- xunit.runner.visualstudio: 2.8.1 → 2.8.*
- Moq: 4.20.70 → 4.20.*

### Fixed
- Nullability warnings in repository pattern
- MovieController null handling for CreateAsync operations
- Docker build configuration and project paths

### Documentation
- Added .NET 10 migration plan documentation
- Added migration task breakdown for sub-agents
- Added migration completion report
- Updated README with .NET 10 prerequisites

### Technical Details
- All 8 unit tests pass
- Docker containers build and run successfully
- EF Core 10 migrations execute correctly
- Swagger UI works with .NET 10
- No breaking changes to API contracts

---

## [1.0.0] - 2026-01-01

### Added
- Initial monorepo structure
- Angular 21 client with SSR
- ASP.NET Core MVC web application
- ASP.NET Core Web API
- Docker Compose orchestration
- CI/CD pipelines
- Comprehensive documentation

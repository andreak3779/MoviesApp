# Changelog

All notable changes to the MoviesApp project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.0.0] - 2026-05-18

### 🛡️ **Security Implementation - OWASP Top 10 Compliant**

#### Added

##### Web API Security Features
- **JWT Authentication** - Complete JWT-based authentication system with refresh tokens
  - Access tokens (15-minute expiration)
  - Refresh tokens (7-day expiration) with rotation
  - Token revocation support
- **ASP.NET Core Identity** - Full user management system
  - User registration with email validation
  - Password policy enforcement (8+ chars, uppercase, lowercase, number)
  - Account lockout after 5 failed attempts
  - Role-based authorization (Admin, User roles)
- **Authorization System**
  - Object-level ownership validation
  - Property-level authorization with separate DTOs
  - Authorization policies (UserOnly, AdminOnly)
- **Rate Limiting**
  - 100 requests/minute for read operations
  - 10 requests/minute for write operations
  - Per-user rate limiting (by user ID or IP)
- **Pagination**
  - Paginated responses for all list endpoints
  - Configurable page size (default: 20, max: 100)
  - Pagination metadata in responses
- **Audit Logging**
  - Automatic logging of all CREATE/UPDATE/DELETE operations
  - Captures user ID, action, entity, values, IP, user agent
  - Queryable audit logs
- **Security Headers Middleware**
  - Content-Security-Policy
  - Strict-Transport-Security (HSTS)
  - X-Frame-Options: DENY
  - X-Content-Type-Options: nosniff
  - X-XSS-Protection
  - Referrer-Policy
  - Permissions-Policy
- **Global Exception Handling**
  - Generic error messages to clients
  - Detailed errors logged internally
  - No stack traces exposed
- **GUID-based Resource IDs**
  - Migrated from integer IDs to GUIDs
  - Prevents resource enumeration attacks
- **API Versioning**
  - Versioned routes: `/api/v1/[controller]`
  - Swagger configured for versioning

##### New Endpoints
- `POST /api/v1/auth/register` - User registration
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh access token
- `POST /api/v1/auth/logout` - Revoke refresh token
- `GET /api/v1/auth/me` - Get current user info

##### New Models
- `ApplicationUser` - Custom Identity user model
- `RefreshToken` - Refresh token entity
- `AuditLog` - Audit log entity

##### New DTOs
- `RegisterDto`, `LoginDto`, `AuthResponseDto`, `RefreshTokenDto`
- `MovieCreateDto`, `MovieUpdateDto`, `MovieResponseDto`
- `PagedResponse<T>`, `PaginationParameters`

##### New Services
- `AuthService` - JWT token generation and validation
- `AuditLogService` - Audit log management

##### Middleware
- `ExceptionHandlingMiddleware` - Global exception handling
- `SecurityHeadersMiddleware` - Security headers
- `RateLimitingMiddleware` - Rate limiting
- `AuditLoggingMiddleware` - Automatic audit logging

#### Changed

##### Breaking Changes
- **Movie IDs changed from `int` to `Guid`**
  - All movie endpoints now use GUIDs
  - Route constraint: `{id:guid}`
  - Database migration required
- **API routes versioned**
  - Changed from `/api/movies` to `/api/v1/movies`
  - All endpoints now require `/v1/` in path
- **Authentication required**
  - All movie endpoints now require JWT token
  - FilmGenres and MediaTypes GET endpoints remain public
- **Ownership validation**
  - Users can only access their own movies
  - `GET /movies` returns only user's movies

#### Database Changes

##### New Tables
- `AspNetUsers` - Identity users
- `AspNetRoles` - Identity roles
- `AspNetUserRoles` - User-role mappings
- `RefreshTokens` - JWT refresh tokens
- `AuditLogs` - Audit trail

##### Schema Changes
- `Movies.Id` changed from `int` to `Guid`
- `Movies.OwnerId` added (references AspNetUsers)
- `MovieMediaTypes.MovieId` changed to `Guid`
- `MovieFilmGenres.MovieId` changed to `Guid`

#### Documentation

##### New Documentation Files
- `apps/web-api/docs/plans/OWASP-TOP10-COMPLIANCE-PLAN.md`
- `apps/web-api/docs/plans/OWASP-SUB-AGENT-TASK-BRIEFS.md`
- `apps/web-api/docs/plans/OWASP-IMPLEMENTATION-PROGRESS.md`
- `apps/web-api/SECURE-CONFIGURATION-GUIDE.md`
- `apps/web-api/IMPLEMENTATION-COMPLETE-SUMMARY.md`
- `docs/SECURITY-OVERVIEW.md`

##### Updated Documentation
- `apps/web-api/README.md` - Complete rewrite with security focus
- `README.md` - Updated with security features
- `docs/ARCHITECTURE.md` - Added security architecture section

#### Dependencies

##### Added
- `Microsoft.AspNetCore.Authentication.JwtBearer` (10.*)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.*)
- `Asp.Versioning.Mvc` (8.*)
- `Asp.Versioning.Mvc.ApiExplorer` (8.*)
- `AutoMapper` (14.0.0)
- `BCrypt.Net-Next` (4.*)

#### Security

##### OWASP Top 10 Coverage
- ✅ API1: Broken Object Level Authorization
- ✅ API2: Broken Authentication
- ✅ API3: Broken Object Property Level Authorization
- ✅ API4: Unrestricted Resource Consumption
- ✅ API5: Broken Function Level Authorization
- ✅ API6: Unrestricted Access to Sensitive Business Flows
- ✅ API8: Security Misconfiguration
- ✅ API9: Improper Inventory Management

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

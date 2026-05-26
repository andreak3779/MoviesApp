# MoviesAPI - OWASP Top 10 Compliant Web API

**Project Structure**: Clean Architecture with separated Domain, Data, and API layers

Secure, production-ready ASP.NET Core Web API for managing movie collections with full OWASP Top 10 API Security compliance.

## 🏗️ Architecture

This solution follows **Clean Architecture** principles with clear separation of concerns:

```
MoviesAPI.sln
├── MoviesApiDomain/     # Domain Layer - Business entities
├── MoviesApiData/       # Data Layer - EF Core, Repositories, Migrations
├── MoviesApi/           # API Layer - Controllers, Services, DTOs
└── MoviesApi.Tests/     # Test Project - Unit tests
```

### Layer Responsibilities

| Project | Purpose | Dependencies |
|---------|---------|--------------|
| **MoviesApiDomain** | Pure domain entities (POCOs) | None |
| **MoviesApiData** | Data access (DbContext, Repositories, Migrations) | MoviesApiDomain |
| **MoviesApi** | Web API (Controllers, Services, DTOs, Auth) | MoviesApiDomain, MoviesApiData |
| **MoviesApi.Tests** | Unit tests | All projects |

### Dependency Flow
```
MoviesApi → MoviesApiData → MoviesApiDomain
```

**Benefits:**
- ✅ Clear separation of concerns
- ✅ Testable components
- ✅ No circular dependencies
- ✅ Easy to maintain and extend
- ✅ Follows SOLID principles

---

## 🔒 Security Features

✅ **OWASP Top 10 API Security Compliant** (8/10 vulnerabilities addressed)

- JWT-based authentication with refresh tokens
- Role-based authorization (Admin, User)
- Object-level ownership validation
- Rate limiting (100 req/min read, 10 req/min write)
- Request pagination
- Comprehensive audit logging
- Security headers (CSP, HSTS, X-Frame-Options, etc.)
- Input validation and sanitization
- GUID-based resource IDs (anti-enumeration)
- Protected Swagger documentation

---

## 📋 Prerequisites

- .NET SDK 10.x
- SQLite (included) or PostgreSQL/MySQL for production
- Node.js (for secret key generation)

---

## 🚀 Quick Start

### 1. Configure Secrets (Required)

```bash
cd /home/user/src/MoviesApp/apps/web-api/MoviesApi

# Initialize user secrets
dotnet user-secrets init

# Generate and set JWT secret key
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 32)"
```

### 2. Restore and Build

```bash
dotnet restore
dotnet build
```

### 3. Run the API

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

---

## 🔐 Authentication Endpoints

### Register New User

```bash
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number

### Login

```bash
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbS...",
  "expiresAt": "2026-05-18T12:15:00Z",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Refresh Token

```bash
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "token": "YOUR_ACCESS_TOKEN",
  "refreshToken": "YOUR_REFRESH_TOKEN"
}
```

### Logout

```bash
POST /api/v1/auth/logout
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "refreshToken": "YOUR_REFRESH_TOKEN"
}
```

### Get Current User

```bash
GET /api/v1/auth/me
Authorization: Bearer YOUR_TOKEN
```

---

## 🎬 Movie Endpoints

All movie endpoints require authentication. Users can only access their own movies.

### Get All Movies (Paginated)

```bash
GET /api/v1/movies?pageNumber=1&pageSize=20
Authorization: Bearer YOUR_TOKEN
```

**Response:**
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 45,
  "totalPages": 3,
  "hasPrevious": false,
  "hasNext": true
}
```

### Get Movie by ID

```bash
GET /api/v1/movies/{guid}
Authorization: Bearer YOUR_TOKEN
```

### Create Movie

```bash
POST /api/v1/movies
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "title": "Inception",
  "releaseYear": 2010,
  "mediaTypeIds": [1, 4],
  "filmGenreIds": [1, 6]
}
```

### Update Movie

```bash
PUT /api/v1/movies/{guid}
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "title": "Inception (Updated)",
  "releaseYear": 2010
}
```

### Delete Movie

```bash
DELETE /api/v1/movies/{guid}
Authorization: Bearer YOUR_TOKEN
```

---

## 📚 Reference Data Endpoints

These endpoints are publicly accessible (no authentication required).

### Get All Media Types

```bash
GET /api/v1/mediatypes
```

### Get Media Type by ID

```bash
GET /api/v1/mediatypes/{id}
```

### Get All Film Genres

```bash
GET /api/v1/filmgenres
```

### Get Film Genre by ID

```bash
GET /api/v1/filmgenres/{id}
```

---

## 🛡️ Security Features

### Rate Limiting

- **Read operations (GET):** 100 requests per minute
- **Write operations (POST/PUT/DELETE):** 10 requests per minute
- Per-user rate limiting (by user ID or IP)
- Returns `429 Too Many Requests` when exceeded
- Includes `Retry-After` header

### Security Headers

All responses include:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Content-Security-Policy: default-src 'self'`
- `Strict-Transport-Security` (production only)
- `Referrer-Policy: strict-origin-when-cross-origin`

### Audit Logging

All CREATE, UPDATE, and DELETE operations are automatically logged with:
- User ID
- Action type
- Entity type and ID
- Old and new values
- IP address
- User agent
- Timestamp (UTC)

---

## 🧪 Running Tests

```bash
cd /home/user/src/MoviesApp/apps/web-api
dotnet test
```

---

## 🔧 Configuration

### Development (User Secrets)

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=movies.db"
```

### Production (Environment Variables)

```bash
export JwtSettings__SecretKey="your-production-secret"
export ConnectionStrings__DefaultConnection="Host=db;Database=movies;Username=user;Password=pass"
```

See `SECURE-CONFIGURATION-GUIDE.md` for complete configuration documentation.

---

## 📊 Database Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update
```

---

## 📁 Project Structure

```
MoviesApi/
├── Controllers/
│   ├── AuthController.cs       # Authentication endpoints
│   ├── MovieController.cs      # Movie CRUD operations
│   ├── FilmGenresController.cs # Reference data
│   └── MediaTypesController.cs # Reference data
├── Data/
│   └── MoviesDbContext.cs      # EF Core DbContext
├── DTOs/
│   ├── AuthDtos.cs             # Authentication DTOs
│   ├── MovieDtos.cs            # Movie DTOs
│   └── PaginationDtos.cs       # Pagination DTOs
├── Middleware/
│   ├── AuditLoggingMiddleware.cs
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RateLimitingMiddleware.cs
│   └── SecurityHeadersMiddleware.cs
├── Models/
│   ├── ApplicationUser.cs      # Identity user model
│   ├── Movie.cs                # Movie entity
│   ├── RefreshToken.cs         # Refresh token entity
│   └── AuditLog.cs             # Audit log entity
├── Repositories/
│   ├── IMovieRepository.cs
│   └── MovieRepository.cs
├── Services/
│   ├── AuthService.cs          # JWT authentication
│   └── AuditLogService.cs      # Audit logging
└── Program.cs                  # Application configuration
```

---

## 📚 Documentation

- **OWASP Compliance Plan:** `docs/plans/OWASP-TOP10-COMPLIANCE-PLAN.md`
- **Implementation Summary:** `IMPLEMENTATION-COMPLETE-SUMMARY.md`
- **Configuration Guide:** `SECURE-CONFIGURATION-GUIDE.md`
- **API Documentation:** Available at `/swagger` (development only)

---

## 🚨 Important Security Notes

1. **Change the JWT Secret Key** before deploying to production
2. **Update CORS origins** in `Program.cs` for your production domain
3. **Use HTTPS** in production (enabled by default)
4. **Monitor audit logs** for suspicious activity
5. **Keep dependencies updated** to address vulnerabilities

---

## 🏆 OWASP Top 10 Coverage

| Vulnerability | Status | Implementation |
|--------------|--------|----------------|
| API1: Broken Object Level Authorization | ✅ Fixed | Ownership validation + GUIDs |
| API2: Broken Authentication | ✅ Fixed | JWT + refresh tokens |
| API3: Broken Object Property Level Authorization | ✅ Fixed | Separate DTOs |
| API4: Unrestricted Resource Consumption | ✅ Fixed | Rate limiting + pagination |
| API5: Broken Function Level Authorization | ✅ Fixed | Authorization policies |
| API6: Unrestricted Access to Sensitive Business Flows | ✅ Fixed | Audit logging |
| API7: SSRF | ⚠️ N/A | No external URL processing |
| API8: Security Misconfiguration | ✅ Fixed | Security headers + CORS |
| API9: Improper Inventory Management | ✅ Fixed | API versioning |
| API10: Unsafe API Consumption | ⚠️ N/A | No external APIs |

---

## 📄 License

This project is for educational purposes.

---

## Workflow for Movies API

This document describes the workflow branching model for managing the development of the Movies API project.

### Branches

#### Main Branches
- **`main`**: The production-ready branch. Always reflects the latest stable release.
- **`develop`**: The integration branch for new features. Contains the latest development changes.

#### Supporting Branches
- **Feature Branches**: Used to develop new features. Merged into `develop`.
  - Naming convention: `feature/<feature-name>`
- **Release Branches**: Used to prepare for a new production release. Merged into both `main` and `develop`.
  - Naming convention: `release/<version-number>`
- **Hotfix Branches**: Used to fix critical issues in production. Merged into both `main` and `develop`.
  - Naming convention: `hotfix/<version-number>`

---

## Workflow

### 1. Start a New Feature
1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git pull
   git checkout -b feature/<feature-name>
   ```
2. Develop the feature and commit changes.

3. Push the branch to the remote repository:
   ```bash
   git push -u origin feature/<feature-name>
   ```

4. When the feature is complete, create a pull request to merge into `develop`.

---

### 2. Start a Release
1. Create a release branch from `develop`:
   ```bash
   git checkout develop
   git pull
   git checkout -b release/<version-number>
   ```

2. Perform final testing, bug fixes, and version updates.

3. Merge the release branch into `main`:
   ```bash
   git checkout main
   git pull
   git merge --no-ff release/<version-number>
   git tag -a v<version-number> -m "Release <version-number>"
   git push origin main --tags
   ```

4. Merge the release branch back into `develop`:
   ```bash
   git checkout develop
   git pull
   git merge --no-ff release/<version-number>
   git push origin develop
   ```

5. Delete the release branch:
   ```bash
   git branch -d release/<version-number>
   git push origin --delete release/<version-number>
   ```

---

### 3. Hotfix Workflow
1. Create a hotfix branch from `main`:
   ```bash
   git checkout main
   git pull
   git checkout -b hotfix/<version-number>
   ```

2. Fix the issue and commit changes.

3. Merge the hotfix branch into `main`:
   ```bash
   git checkout main
   git pull
   git merge --no-ff hotfix/<version-number>
   git tag -a v<version-number> -m "Hotfix <version-number>"
   git push origin main --tags
   ```

4. Merge the hotfix branch back into `develop`:
   ```bash
   git checkout develop
   git pull
   git merge --no-ff hotfix/<version-number>
   git push origin develop
   ```

5. Delete the hotfix branch:
   ```bash
   git branch -d hotfix/<version-number>
   git push origin --delete hotfix/<version-number>
   ```

---

## Best Practices
- Keep commits small and focused.
- Write meaningful commit messages.
- Regularly pull changes from `develop` to avoid merge conflicts.
- Ensure all tests pass before merging into `develop` or `main`.

---

## Example Commands

### Create a Feature Branch
```bash
git checkout -b feature/add-movie-endpoint
```

### Merge a Feature Branch
```bash
git checkout develop
git merge --no-ff feature/add-movie-endpoint
git push origin develop
```

### Start a Release
```bash
git checkout -b release/1.0.0
```

### Apply a Hotfix
```bash
git checkout -b hotfix/1.0.1
```

---

## References
- [GitFlow Workflow](https://nvie.com/posts/a-successful-git-branching-model/)
- [Git Documentation](https://git-scm.com/doc)

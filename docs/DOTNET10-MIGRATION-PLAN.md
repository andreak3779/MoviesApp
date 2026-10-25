# .NET 10 Migration Plan

> **Historical document — do not treat as current state.** This file is kept
> for the record. The authoritative snapshot of the codebase is
> [PROJECT-STATUS.md](PROJECT-STATUS.md); the current system shape is in
> [ARCHITECTURE.md](ARCHITECTURE.md). Where this document disagrees with those
> two, they are right.

## Overview
This document outlines the steps to migrate the MoviesApp to .NET 10, ASP.NET Core 10, C# 10, and EF Core 10.

## Current State (as of May 2026)

### Projects Status
- **MoviesWeb (MVC)**: Already targeting `net10.0` ✅
- **MoviesWeb.Tests**: Already targeting `net10.0` ✅
- **MoviesApi**: Targeting `net9.0` with multi-targeting `net9.0;net8.0` ❌
- **MoviesApi.Tests**: Already targeting `net10.0` ✅
- **.NET SDK Installed**: 10.0.107 ✅

### Package Versions
- **EF Core**: 9.* (needs update to 10.*)
- **Microsoft.AspNetCore.Mvc.NewtonsoftJson**: 6.0.0 (needs update)
- **Swashbuckle.AspNetCore**: 6.0.0 (needs update)
- **Test packages**: Mostly current

### Dockerfiles
- **mvc-web/Dockerfile**: Using .NET 10.0 ✅
- **web-api/Dockerfile**: Using .NET 10.0 SDK but runtime 9.0, wrong project path ❌
- **angular-client/Dockerfile**: Node-based, no changes needed ✅

---

## Migration Tasks

### Phase 1: Update MoviesApi Project

#### 1.1 Update Target Framework
**File**: `apps/web-api/MoviesApi/MoviesApi.csproj`

**Changes**:
- Change `TargetFramework` from `net9.0` to `net10.0`
- Update `TargetFrameworks` from `net9.0;net8.0` to `net10.0` (or remove multi-targeting)

#### 1.2 Update Entity Framework Core Packages
**File**: `apps/web-api/MoviesApi/MoviesApi.csproj`

**Changes**:
- `Microsoft.EntityFrameworkCore`: 9.* → 10.*
- `Microsoft.EntityFrameworkCore.Design`: 9.* → 10.*
- `Microsoft.EntityFrameworkCore.Sqlite`: 9.* → 10.*
- `Microsoft.EntityFrameworkCore.Tools`: 9.* → 10.*

#### 1.3 Update Other Packages
**File**: `apps/web-api/MoviesApi/MoviesApi.csproj`

**Changes**:
- `Microsoft.AspNetCore.Mvc.NewtonsoftJson`: 6.0.0 → 10.*
- `Swashbuckle.AspNetCore`: 6.0.0 → 7.* (or latest compatible with .NET 10)

#### 1.4 Fix Dockerfile
**File**: `apps/web-api/Dockerfile`

**Changes**:
- Update runtime from `mcr.microsoft.com/dotnet/aspnet:9.0` to `mcr.microsoft.com/dotnet/aspnet:10.0`
- Fix publish command to reference correct project: `MoviesApi/MoviesApi.csproj` instead of `MovieCollectionApi/MovieCollectionApi.csproj`
- Update DLL name in CMD if necessary

---

### Phase 2: Code Compatibility Review

#### 2.1 Review Breaking Changes
Check for breaking changes in:
- **ASP.NET Core 10**: Review migration guides for deprecated APIs
- **EF Core 10**: Check for deprecated methods, changed behaviors
- **C# 10**: Ensure code is compatible (should be forward compatible)

#### 2.2 Files to Review
- `apps/web-api/MoviesApi/Program.cs`
- `apps/mvc-web/MoviesWeb/Program.cs`
- All Controllers in both projects
- All Services and Repositories
- Data/DbContext classes

#### 2.3 Common Breaking Changes to Watch For
- Minimal API changes
- Authentication/Authorization changes
- EF Core query behavior changes
- JSON serialization changes
- Deprecated attributes/methods

---

### Phase 3: Update Test Projects

#### 3.1 MoviesApi.Tests
**File**: `apps/web-api/MoviesApi.Tests/MoviesApi.Tests.csproj`

**Status**: Already on net10.0 ✅

**Actions**:
- Update test packages if needed
- Ensure project reference works with updated MoviesApi

#### 3.2 MoviesWeb.Tests
**File**: `apps/mvc-web/MoviesWeb.Tests/MoviesWeb.Tests.csproj`

**Status**: Already on net10.0 ✅

**Actions**:
- Update test packages if needed
- Ensure all tests pass with updated MoviesWeb

---

### Phase 4: Update Solution Files

#### 4.1 Solution Files
**Files**:
- `apps/web-api/MoviesAPI.sln`
- `apps/mvc-web/MoviesWeb.sln`

**Actions**:
- Ensure solution files reference correct projects
- Update any solution-level configurations

---

### Phase 5: Build and Test

#### 5.1 Build Order
1. Build MoviesApi
2. Build MoviesApi.Tests
3. Build MoviesWeb
4. Build MoviesWeb.Tests

#### 5.2 Commands
```bash
# Web API
cd apps/web-api
dotnet restore MoviesAPI.sln
dotnet build MoviesAPI.sln -c Release

# MVC Web
cd apps/mvc-web
dotnet restore MoviesWeb.sln
dotnet build MoviesWeb.sln -c Release
```

#### 5.3 Run Tests
```bash
# Web API Tests
cd apps/web-api
dotnet test MoviesApi.Tests/MoviesApi.Tests.csproj

# MVC Web Tests
cd apps/mvc-web
dotnet test MoviesWeb.Tests/MoviesWeb.Tests.csproj
```

---

### Phase 6: Docker Updates

#### 6.1 Update Docker Compose
**File**: `docker-compose.yml`

**Actions**:
- Verify all services work with .NET 10
- Update any environment variables if needed

#### 6.2 Build and Test Docker Images
```bash
# Build all services
docker-compose build

# Run services
docker-compose up
```

---

### Phase 7: Integration Testing

#### 7.1 Manual Testing Checklist
- [ ] MoviesApi health endpoint: `http://localhost:8081/health`
- [ ] MoviesWeb health endpoint: `http://localhost:8080/health`
- [ ] MoviesWeb can fetch movies from MoviesApi
- [ ] All CRUD operations work
- [ ] Swagger UI works for MoviesApi
- [ ] Database migrations work correctly

#### 7.2 Angular Client Integration
- [ ] Angular client can connect to updated API
- [ ] Test all features in browser

---

### Phase 8: Documentation Updates

#### 8.1 Update README Files
- `apps/web-api/README.md`
- `apps/mvc-web/README.md`
- `README.md` (root)

#### 8.2 Update Architecture Documentation
- `docs/ARCHITECTURE.md`
- `docs/CI-CD.md`

---

## Package Version Reference (.NET 10)

### Recommended Versions (as of May 2026)
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="10.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.*" />
```

### Test Packages
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.*" />
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.*" />
<PackageReference Include="Moq" Version="4.20.*" />
<PackageReference Include="coverlet.collector" Version="6.0.*" />
```

---

## Rollback Plan

If issues arise:
1. Revert package versions in `.csproj` files
2. Restore previous Dockerfile versions
3. Use .NET 9 SDK if needed
4. Check EF Core migration compatibility

---

## Success Criteria

- [ ] All projects build without errors
- [ ] All unit tests pass
- [ ] Docker containers start successfully
- [ ] API endpoints respond correctly
- [ ] MVC web application functions properly
- [ ] Angular client integrates successfully
- [ ] No runtime errors in logs
- [ ] Performance is acceptable

---

## Timeline Estimate

- **Phase 1-2**: 1-2 hours
- **Phase 3-4**: 30 minutes
- **Phase 5**: 1 hour
- **Phase 6**: 30 minutes
- **Phase 7**: 1-2 hours
- **Phase 8**: 30 minutes

**Total**: 4-7 hours (including testing and troubleshooting)

---

## Notes

- .NET 10 is LTS (Long Term Support)
- C# 10 features are optional - existing C# code should work
- EF Core 10 maintains most EF Core 9 APIs
- Always backup before making changes
- Test in development before production deployment

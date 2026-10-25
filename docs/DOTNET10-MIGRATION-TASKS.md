# .NET 10 Migration - Sub-Agent Task Breakdown

> **Historical document — do not treat as current state.** This file is kept
> for the record. The authoritative snapshot of the codebase is
> [PROJECT-STATUS.md](PROJECT-STATUS.md); the current system shape is in
> [ARCHITECTURE.md](ARCHITECTURE.md). Where this document disagrees with those
> two, they are right.

## Overview
This document breaks down the .NET 10 migration into discrete tasks that can be assigned to sub-agents for parallel execution.

## Task Distribution

---

## Task 1: Update MoviesApi Project File
**Agent**: Package Update Specialist  
**Priority**: HIGH  
**Dependencies**: None  
**Files**: `apps/web-api/MoviesApi/MoviesApi.csproj`

### Instructions
Update the MoviesApi project file to target .NET 10 and update all package references.

### Specific Changes Required
1. Change `TargetFramework` from `net9.0` to `net10.0`
2. Remove or update `TargetFrameworks` from `net9.0;net8.0` to just `net10.0`
3. Update package versions:
   - `Microsoft.EntityFrameworkCore`: 9.* → 10.*
   - `Microsoft.EntityFrameworkCore.Design`: 9.* → 10.*
   - `Microsoft.EntityFrameworkCore.Sqlite`: 9.* → 10.*
   - `Microsoft.EntityFrameworkCore.Tools`: 10.*
   - `Microsoft.AspNetCore.Mvc.NewtonsoftJson`: 6.0.0 → 10.*
   - `Swashbuckle.AspNetCore`: 6.0.0 → 7.* (or latest compatible)

### Success Criteria
- Project file targets net10.0
- All packages use .NET 10 compatible versions
- No build errors after changes

### Command to Verify
```bash
cd apps/web-api
dotnet build MoviesApi/MoviesApi.csproj
```

---

## Task 2: Fix Web API Dockerfile
**Agent**: Docker Specialist  
**Priority**: HIGH  
**Dependencies**: Task 1 completion preferred but not required  
**Files**: `apps/web-api/Dockerfile`

### Instructions
Fix the Web API Dockerfile to use correct .NET 10 runtime and fix project path errors.

### Specific Changes Required
1. Update runtime stage FROM:
   - FROM: `mcr.microsoft.com/dotnet/aspnet:9.0`
   - TO: `mcr.microsoft.com/dotnet/aspnet:10.0`
2. Fix publish command project path:
   - FROM: `MovieCollectionApi/MovieCollectionApi.csproj`
   - TO: `MoviesApi/MoviesApi.csproj`
3. Fix publish command framework flag:
   - FROM: `-f net9.0`
   - TO: `-f net10.0` (or remove if single-target)
4. Fix CMD DLL name if needed:
   - FROM: `MovieCollectionApi.dll`
   - TO: `MoviesApi.dll`

### Success Criteria
- Dockerfile uses .NET 10 for both build and runtime
- Project paths are correct
- Docker build succeeds

### Command to Verify
```bash
cd apps/web-api
docker build -t movies-api:test .
```

---

## Task 3: Update Test Project Packages
**Agent**: Testing Specialist  
**Priority**: MEDIUM  
**Dependencies**: Task 1 completion  
**Files**: 
- `apps/web-api/MoviesApi.Tests/MoviesApi.Tests.csproj`
- `apps/mvc-web/MoviesWeb.Tests/MoviesWeb.Tests.csproj`

### Instructions
Review and update test project package references to latest compatible versions.

### Specific Changes Required
Check and update if needed:
- `Microsoft.NET.Test.Sdk`: to 17.12.*
- `xunit`: to 2.9.*
- `xunit.runner.visualstudio`: to 2.8.*
- `Moq`: to 4.20.*
- `coverlet.collector`: to 6.0.*

Ensure project references to main projects still work.

### Success Criteria
- Test packages are up to date
- Tests compile without errors
- Project references resolve correctly

### Command to Verify
```bash
cd apps/web-api
dotnet build MoviesApi.Tests/MoviesApi.Tests.csproj

cd apps/mvc-web
dotnet build MoviesWeb.Tests/MoviesWeb.Tests.csproj
```

---

## Task 4: Code Compatibility Review
**Agent**: Code Review Specialist  
**Priority**: HIGH  
**Dependencies**: Task 1 completion  
**Files**: All .cs files in both projects

### Instructions
Review code for .NET 10, ASP.NET Core 10, and EF Core 10 compatibility issues.

### Files to Review
- `apps/web-api/MoviesApi/Program.cs`
- `apps/mvc-web/MoviesWeb/Program.cs`
- All Controllers in both projects
- All Services and Repositories
- Data/DbContext classes
- Any configuration files

### What to Look For
1. **Deprecated APIs**: Check for obsolete methods/attributes
2. **Breaking Changes**: Review .NET 10 migration guides
3. **EF Core Changes**: Query behavior, method signatures
4. **ASP.NET Core Changes**: Middleware, routing, authentication
5. **JSON Serialization**: System.Text.Json compatibility
6. **Minimal API Changes**: If applicable

### Actions
- Document any issues found
- Fix compatibility issues
- Update code to use recommended patterns
- Add comments for any workarounds needed

### Success Criteria
- No deprecated API usage
- Code compiles without warnings
- No breaking change issues

---

## Task 5: Build and Unit Testing
**Agent**: Build & Test Specialist  
**Priority**: HIGH  
**Dependencies**: Tasks 1, 2, 3, 4 completion  
**Files**: All projects

### Instructions
Perform full build and run all unit tests for both solutions.

### Build Steps
```bash
# Web API Solution
cd apps/web-api
dotnet restore MoviesAPI.sln
dotnet build MoviesAPI.sln -c Release

# MVC Web Solution
cd apps/mvc-web
dotnet restore MoviesWeb.sln
dotnet build MoviesWeb.sln -c Release
```

### Test Steps
```bash
# Web API Tests
cd apps/web-api
dotnet test MoviesApi.Tests/MoviesApi.Tests.csproj --verbosity normal

# MVC Web Tests
cd apps/mvc-web
dotnet test MoviesWeb.Tests/MoviesWeb.Tests.csproj --verbosity normal
```

### Success Criteria
- Both solutions build without errors
- All unit tests pass
- No critical warnings
- Code coverage is acceptable (>80% if tracked)

### Deliverables
- Build output logs
- Test results summary
- List of any failures with details

---

## Task 6: Docker Compose Integration Testing
**Agent**: DevOps Specialist  
**Priority**: MEDIUM  
**Dependencies**: Tasks 1, 2, 5 completion  
**Files**: 
- `docker-compose.yml`
- All Dockerfiles

### Instructions
Build and test all Docker containers to ensure they work together.

### Build Steps
```bash
cd /home/user/src/MoviesApp
docker-compose build
```

### Run Steps
```bash
docker-compose up -d
```

### Verification Steps
```bash
# Check all containers are running
docker-compose ps

# Test MoviesApi health
curl http://localhost:8081/health

# Test MoviesWeb health
curl http://localhost:8080/health

# Check logs
docker-compose logs web-api
docker-compose logs mvc-web
```

### Success Criteria
- All containers build successfully
- All containers start without errors
- Health endpoints respond
- Containers can communicate with each other
- No runtime errors in logs

### Cleanup
```bash
docker-compose down
```

---

## Task 7: Documentation Updates
**Agent**: Documentation Specialist  
**Priority**: LOW  
**Dependencies**: All other tasks completion  
**Files**: 
- `README.md` (root)
- `apps/web-api/README.md`
- `apps/mvc-web/README.md`
- `docs/ARCHITECTURE.md`

### Instructions
Update all documentation to reflect .NET 10 migration.

### Content to Update
1. **Prerequisites**: Update .NET SDK version requirement to 10.0
2. **Build Instructions**: Update any version-specific commands
3. **Docker Instructions**: Update image tags if mentioned
4. **Architecture Docs**: Note the .NET 10 upgrade
5. **Changelog**: Add entry for .NET 10 migration

### Success Criteria
- All docs reference correct .NET version
- Build/run instructions are accurate
- Migration is documented in changelog

---

## Task 8: Final Integration Testing
**Agent**: QA Specialist  
**Priority**: HIGH  
**Dependencies**: Tasks 5, 6 completion  
**Files**: N/A (manual testing)

### Instructions
Perform comprehensive integration testing of the entire application stack.

### Test Scenarios

#### API Tests
- [ ] GET /health endpoint
- [ ] GET /api/movies (list all)
- [ ] GET /api/movies/{id} (single item)
- [ ] POST /api/movies (create)
- [ ] PUT /api/movies/{id} (update)
- [ ] DELETE /api/movies/{id} (delete)
- [ ] Swagger UI loads at /swagger

#### MVC Web Tests
- [ ] Home page loads
- [ ] Movies list page works
- [ ] Movie details page works
- [ ] Create movie form works
- [ ] Edit movie form works
- [ ] Delete confirmation works

#### Angular Client Tests (if applicable)
- [ ] Angular app loads
- [ ] Can fetch movies from API
- [ ] All UI features work

### Success Criteria
- All test scenarios pass
- No errors in application logs
- Performance is acceptable
- User experience is not degraded

---

## Execution Strategy

### Phase 1: Foundation (Parallel)
- Task 1: Update MoviesApi Project File
- Task 2: Fix Web API Dockerfile
- Task 3: Update Test Project Packages

### Phase 2: Code Review (After Phase 1)
- Task 4: Code Compatibility Review

### Phase 3: Build & Test (After Phase 2)
- Task 5: Build and Unit Testing
- Task 6: Docker Compose Integration Testing

### Phase 4: Finalization (After Phase 3)
- Task 7: Documentation Updates
- Task 8: Final Integration Testing

---

## Coordination Notes

### Communication
- Each sub-agent should report:
  - Tasks completed
  - Issues encountered
  - Files modified
  - Test results

### Rollback Plan
If any task fails:
1. Document the issue
2. Revert changes made by that task
3. Report to coordinator
4. Await further instructions

### Success Definition
Migration is complete when:
- ✅ All 8 tasks completed successfully
- ✅ All tests pass
- ✅ Documentation updated
- ✅ No critical issues remain

---

## Quick Reference

### Project Paths
- Web API: `/home/user/src/MoviesApp/apps/web-api/`
- MVC Web: `/home/user/src/MoviesApp/apps/mvc-web/`
- Angular: `/home/user/src/MoviesApp/apps/angular-client/`

### Key Commands
```bash
# Build
dotnet build <project>.csproj

# Test
dotnet test <test-project>.csproj

# Docker
docker-compose build
docker-compose up -d
docker-compose down
```

### Package Versions (.NET 10)
- EF Core: 10.*
- ASP.NET Core: 10.*
- Swashbuckle: 7.*
- xUnit: 2.9.*
- Moq: 4.20.*

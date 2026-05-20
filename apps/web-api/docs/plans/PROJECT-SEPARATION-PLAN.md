# Project Separation Plan: MoviesApi → Domain + Data + API

**Location**: `apps/web-api/docs/plans/PROJECT-SEPARATION-PLAN.md`  
**Status**: Phase 1 Complete - Ready for Parallel Execution (Phase 2)  
**Last Updated**: 19 May 2026

## Overview
Separate the monolithic MoviesApi project into three layered projects following clean architecture principles.

**Related Documents**:
- See `docs/ARCHITECTURE.md` for overall solution architecture
- See `docs/CONTRIBUTING-RUNBOOK.md` for development workflows

## Target Architecture

### 1. MoviesApiDomain (Class Library)
**Purpose**: Pure domain entities with no external framework dependencies  
**Dependencies**: None (pure .NET)  
**Contains**:
- `Models/` - All entity classes:
  - `Movie.cs`
  - `MediaType.cs`
  - `FilmGenre.cs`
  - `MovieMediaType.cs`
  - `MovieFilmGenre.cs`
  - `ApplicationUser.cs`
  - `RefreshToken.cs`
  - `AuditLog.cs`

**Notes**: 
- Entities can keep data annotations (`[Required]`, `[StringLength]`, etc.)
- Remove any EF Core-specific fluent API configurations (those stay in DbContext)
- This project should be referenceable by any layer without causing circular dependencies

---

### 2. MoviesApiData (Class Library)
**Purpose**: Data access layer with EF Core  
**Dependencies**: 
- MoviesApiDomain
- Microsoft.EntityFrameworkCore
- Microsoft.AspNetCore.Identity.EntityFrameworkCore

**Contains**:
- `MoviesDbContext.cs` - DbContext with all configurations
- `Migrations/` - All EF Core migration files
- `movies.db` - SQLite database file
- `Repositories/` - Repository pattern implementations:
  - `IMovieRepository.cs`
  - `MovieRepository.cs`
  - `IMediaTypeRepository.cs`
  - `MediaTypeRepository.cs`
  - `IFilmGenreRepository.cs`
  - `FilmGenreRepository.cs`

**Notes**:
- Repositories belong here because they directly depend on DbContext
- This layer encapsulates all data access logic
- Should not reference the API project (no upward dependencies)

---

### 3. MoviesApi (ASP.NET Core Web API)
**Purpose**: Web API presentation layer  
**Dependencies**:
- MoviesApiDomain
- MoviesApiData
- Microsoft.AspNetCore.Authentication.JwtBearer
- Other ASP.NET Core packages

**Contains**:
- `Controllers/` - All API controllers (consolidated from `Controllers/` and `controllers/`)
  - `MovieController.cs`
  - `FilmGenresController.cs`
  - `MediaTypesController.cs`
  - `AuthController.cs`
- `DTOs/` - Data transfer objects:
  - `AuthDtos.cs` (RegisterDto, LoginDto, AuthResponseDto, etc.)
  - `MovieDtos.cs` (MovieCreateDto, MovieUpdateDto, MovieResponseDto, etc.)
  - `PaginationDtos.cs`
- `Services/` - Application services:
  - `AuthService.cs` + `IAuthService`
  - `AuditLogService.cs`
- `Middleware/` - Custom middleware
- `Program.cs` - Application entry point
- `appsettings.json` - Configuration
- `Properties/` - Launch settings

**Notes**:
- DTOs belong here as they are API contracts
- Services orchestrate between controllers and repositories
- This is the composition root that wires up all dependencies
- **Consolidation**: Merge both `Controllers/` and `controllers/` folders into single `Controllers/` folder (case-sensitive)

---

## Implementation Strategy: Parallel Execution

### Dependency Analysis

```
Task Dependencies:
┌─────────────────────────────────────────────────────────────┐
│ Phase 1: Projects (✅ COMPLETED)                            │
│  - MoviesApiDomain created                                  │
│  - MoviesApiData created                                    │
└─────────────────────────────────────────────────────────────┘
                          ↓
        ┌─────────────────┼─────────────────┐
        ↓                 ↓                 ↓
   Phase 2A          Phase 2B          Phase 2C
   (Domain)         (Data Layer)      (Test Project)
   Independent      Independent       Independent
        │                 │                 │
        └─────────────────┼─────────────────┘
                          ↓
              Phase 3: API Layer Update
              (depends on 2A + 2B)
                          ↓
              Phase 4: Program.cs & DI
              (depends on Phase 3)
                          ↓
              Phase 5: Build & Verify
              (depends on Phase 4)
                          ↓
              Phase 6: Cleanup
              (final step)
```

**Parallel Execution Benefits:**
- **3 sub-agents can work simultaneously** on Tasks 2A, 2B, 2C
- **~60% time savings**: Parallel tasks complete in ~10 mins vs 25+ mins sequential
- **Clear ownership**: Each sub-agent has well-defined scope
- **Reduced context switching**: Main agent coordinates, sub-agents execute

---

## Implementation Steps (Optimized for Parallel Execution)

### Phase 1: Create New Projects ✅ COMPLETED
- [x] `MoviesApiDomain` class library project created
- [x] `MoviesApiData` class library project created
- [x] Projects added to solution

---

### Phase 2: Parallel Tasks (Can run simultaneously)

#### Task 2A: Domain Layer Setup 🔄
**Sub-agent**: Domain Migration Specialist  
**Dependencies**: None (can start immediately)  
**Files affected**: `MoviesApi/Models/` → `MoviesApiDomain/Models/`

**Tasks:**
1. Create `MoviesApiDomain/Models/` directory
2. Move all entity files:
   - `Movie.cs`
   - `MediaType.cs`
   - `FilmGenre.cs`
   - `MovieMediaType.cs`
   - `MovieFilmGenre.cs`
   - `ApplicationUser.cs`
   - `RefreshToken.cs`
   - `AuditLog.cs`
3. Update namespaces in all files: `MoviesApi.Models` → `MoviesApiDomain.Models`
4. Update `MoviesApiDomain.csproj`:
   ```xml
   <ItemGroup>
     <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
   </ItemGroup>
   ```
5. Verify no EF Core-specific code in entities (only data annotations allowed)
6. Build MoviesApiDomain: `dotnet build MoviesApiDomain/MoviesApiDomain.csproj`

**Success criteria**: MoviesApiDomain builds without errors

---

#### Task 2B: Data Layer Setup 🔄
**Sub-agent**: Data Access Specialist  
**Dependencies**: None (can start immediately)  
**Files affected**: `MoviesApi/Data/`, `MoviesApi/Migrations/`, `MoviesApi/Repositories/`

**Tasks:**
1. Create directory structure in MoviesApiData:
   - `MoviesApiData/Data/`
   - `MoviesApiData/Repositories/`
   - `MoviesApiData/Migrations/`
2. Move files:
   - `MoviesDbContext.cs` → `MoviesApiData/Data/`
   - All `*.cs` from `Migrations/` → `MoviesApiData/Migrations/`
   - `movies.db` → `MoviesApiData/`
   - All repository files → `MoviesApiData/Repositories/`
3. Update namespaces:
   - `MoviesApi.Data` → `MoviesApiData.Data`
   - `MoviesApi.Repositories` → `MoviesApiData.Repositories`
   - Update using statements in DbContext and Repositories
4. Update `MoviesApiData.csproj`:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\MoviesApiDomain\MoviesApiDomain.csproj" />
   </ItemGroup>
   <ItemGroup>
     <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.x" />
     <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.x" />
     <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.x" />
   </ItemGroup>
   ```
5. Update using statements in DbContext: `using MoviesApiDomain.Models;`
6. Build MoviesApiData: `dotnet build MoviesApiData/MoviesApiData.csproj`

**Success criteria**: MoviesApiData builds without errors

---

#### Task 2C: Test Project Update 🔄
**Sub-agent**: Testing Specialist  
**Dependencies**: None (can start immediately)  
**Files affected**: `MoviesApi.Tests/`

**Tasks:**
1. Update `MoviesApi.Tests.csproj`:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\MoviesApi\MoviesApi.csproj" />
     <ProjectReference Include="..\MoviesApiData\MoviesApiData.csproj" />
     <ProjectReference Include="..\MoviesApiDomain\MoviesApiDomain.csproj" />
   </ItemGroup>
   ```
2. Update using statements in test files:
   - `using MoviesApi.Models;` → `using MoviesApiDomain.Models;`
   - `using MoviesApi.Repositories;` → `using MoviesApiData.Repositories;`
3. Verify test mocks reference correct types
4. Build test project: `dotnet build MoviesApi.Tests/MoviesApi.Tests.csproj`

**Success criteria**: Test project builds without errors

---

### Phase 3: API Layer Update (Sequential - waits for 2A, 2B, 2C)

**Prerequisites**: Tasks 2A, 2B, 2C must be complete  
**Files affected**: `MoviesApi/`, `MoviesApi.Tests/`

**Tasks:**
1. Update `MoviesApi.csproj`:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\MoviesApiDomain\MoviesApiDomain.csproj" />
     <ProjectReference Include="..\MoviesApiData\MoviesApiData.csproj" />
   </ItemGroup>
   ```
2. Remove redundant package references (if any moved to Data layer)
3. Update all using statements across API project:
   - Controllers: `using MoviesApiDomain.Models;`, `using MoviesApiData.Repositories;`
   - Services: `using MoviesApiDomain.Models;`, `using MoviesApiData.Data;`
   - DTOs: `using MoviesApiDomain.Models;` (if referencing entities)
4. Consolidate controllers:
   - Move `controllers/*.cs` → `Controllers/`
   - Delete empty `controllers/` folder
5. Build MoviesApi: `dotnet build MoviesApi/MoviesApi.csproj`

**Success criteria**: MoviesApi builds without errors

---

### Phase 4: Program.cs & DI Configuration (Sequential)

**Prerequisites**: Phase 3 complete  
**Files affected**: `MoviesApi/Program.cs`

**Tasks:**
1. Update using statements in Program.cs:
   ```csharp
   using MoviesApiDomain.Models;
   using MoviesApiData.Data;
   using MoviesApiData.Repositories;
   ```
2. Verify DbContext registration:
   ```csharp
   builder.Services.AddDbContext<MoviesDbContext>(options =>
       options.UseSqlite(connectionString));
   ```
3. Verify repository registrations:
   ```csharp
   builder.Services.AddScoped<IMovieRepository, MovieRepository>();
   builder.Services.AddScoped<IMediaTypeRepository, MediaTypeRepository>();
   builder.Services.AddScoped<IFilmGenreRepository, FilmGenreRepository>();
   ```
4. Build solution: `dotnet build MoviesAPI.sln`

**Success criteria**: Solution builds without errors, DI resolves all services

---

### Phase 5: Build & Integration Test (Sequential)

**Prerequisites**: Phase 4 complete  
**Files affected**: Entire solution

**Tasks:**
1. Clean solution: `dotnet clean MoviesAPI.sln`
2. Rebuild all: `dotnet build MoviesAPI.sln`
3. Run unit tests: `dotnet test MoviesApi.Tests/MoviesApi.Tests.csproj`
4. Fix any failing tests
5. Run API: `dotnet run --project MoviesApi/MoviesApi.csproj`
6. Test critical endpoints manually or with integration tests

**Success criteria**: All tests pass, API runs successfully

---

### Phase 6: Cleanup & Documentation (Final)

**Prerequisites**: Phase 5 complete  
**Files affected**: Entire solution

**Tasks:**
1. Remove empty folders from MoviesApi:
   - `Models/`
   - `Data/`
   - `Repositories/`
   - `controllers/` (if not already done)
2. Update README files in each project
3. Update solution-level documentation
4. Update migration commands in docs
5. Commit changes to version control

**Success criteria**: Clean project structure, updated documentation

---

## Namespace Mapping

| Old Namespace | New Namespace | Target Project |
|--------------|---------------|----------------|
| `MoviesApi.Models` | `MoviesApiDomain.Models` | MoviesApiDomain |
| `MoviesApi.Data` | `MoviesApiData.Data` | MoviesApiData |
| `MoviesApi.Repositories` | `MoviesApiData.Repositories` | MoviesApiData |
| `MoviesApi.DTOs` | `MoviesApi.DTOs` | MoviesApi (unchanged) |
| `MoviesApi.Services` | `MoviesApi.Services` | MoviesApi (unchanged) |
| `MoviesApi.Controllers` | `MoviesApi.Controllers` | MoviesApi (unchanged) |

---

## EF Core Best Practices for Project Separation

### Recommended Architecture Pattern: **Domain Entities with Separate Configuration**

Following Microsoft's EF Core documentation and industry best practices, here's the recommended approach:

#### ✅ **DO: Keep DbContext in Data Layer**
- `MoviesDbContext` belongs in `MoviesApiData` project
- All entity configurations (Fluent API) stay in `OnModelCreating`
- Migrations project should be `MoviesApiData`

#### ✅ **DO: Use Pure POCOs in Domain Layer**
- Entity classes in `MoviesApiDomain` should be **Plain Old CLR Objects**
- Keep data annotations for validation (`[Required]`, `[StringLength]`)
- Avoid EF Core-specific attributes or base classes in domain entities
- Domain entities should have **no knowledge of EF Core**

#### ✅ **DO: Configure Relationships in DbContext**
```csharp
// In MoviesDbContext.cs (MoviesApiData project)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configure relationships, keys, indexes here
    // NOT in entity classes
    modelBuilder.Entity<Movie>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasMany(m => m.MovieMediaTypes)
              .WithOne(mm => mm.Movie)
              .HasForeignKey(mm => mm.MovieId);
    });
}
```

#### ✅ **DO: Use Repository Pattern Correctly**
- Repositories in `MoviesApiData` depend on `MoviesDbContext`
- Interfaces (`IMovieRepository`) stay with implementations
- Controllers depend on repository interfaces, not concrete classes
- Enables easier unit testing with mocked repositories

#### ✅ **DO: Keep DTOs Separate from Domain Models**
- DTOs in `MoviesApi` (presentation layer)
- Never expose domain entities directly to API consumers
- Use AutoMapper or manual mapping between Domain ↔ DTO
- Prevents over-posting and circular references

#### ✅ **DO: Manage Migrations Properly**
```bash
# Always specify the startup project and migrations project
dotnet ef migrations add MigrationName \
  --project MoviesApiData \
  --startup-project MoviesApi

dotnet ef database update \
  --project MoviesApiData \
  --startup-project MoviesApi
```

#### ✅ **DO: Configure Dependency Injection in API Layer**
```csharp
// In Program.cs (MoviesApi project)
builder.Services.AddDbContext<MoviesDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
```

#### ❌ **DON'T: Create Circular Dependencies**
```
MoviesApi → MoviesApiData → MoviesApiDomain
     ↓            ↓              ↓
  (depends on) (depends on)   (no dependencies)
```
- MoviesApiData should **never** reference MoviesApi
- MoviesApiDomain should **never** reference EF Core packages

#### ❌ **DON'T: Put Business Logic in Entities**
- Entities are data containers, not business logic holders
- Keep business logic in Services layer (MoviesApi project)
- Prevents anemic domain model anti-pattern

#### ❌ **DON'T: Expose DbContext to Controllers**
- Controllers should depend on repositories or services
- Never inject `MoviesDbContext` directly into controllers
- Maintains separation of concerns

### Alternative: Split Configuration Files (Advanced)

For even cleaner separation, consider moving Fluent API configurations to separate classes:

```
MoviesApiData/
  └── Configurations/
      ├── MovieConfiguration.cs
      ├── MediaTypeConfiguration.cs
      └── FilmGenreConfiguration.cs
```

```csharp
// MovieConfiguration.cs
public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");
        builder.HasKey(e => e.Id);
        // ... configuration
    }
}

// In MoviesDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new MovieConfiguration());
    modelBuilder.ApplyConfiguration(new MediaTypeConfiguration());
    // ...
}
```

**Benefits:**
- Reduces DbContext size
- Easier to maintain and test configurations
- Clearer separation of concerns

**For this project:** Keep configurations in `OnModelCreating` for simplicity unless the file grows beyond 500+ lines.

### Connection String Management

```json
// appsettings.json (MoviesApi project)
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/path/to/MoviesApiData/movies.db"
  }
}
```

Or use relative path resolution in `Program.cs`:
```csharp
var dbPath = Path.Combine(
    AppContext.BaseDirectory, 
    "..", "..", "..", 
    "MoviesApiData", 
    "movies.db"
);
```

### Testing Best Practices

1. **Unit Tests** (MoviesApi.Tests):
   - Mock repositories, don't use real DbContext
   - Test controllers in isolation
   - Test services with mocked repositories

2. **Integration Tests** (Optional separate project):
   - Use real DbContext with in-memory or test database
   - Test full request pipeline
   - Use WebApplicationFactory for end-to-end tests

```csharp
// Unit test example
public class MovieControllerTests
{
    private readonly Mock<IMovieRepository> _mockRepo;
    
    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockRepo.Setup(r => r.GetAllAsync())
                 .ReturnsAsync(new List<Movie> { ... });
        
        // Test controller logic
    }
}
```

---

## Package References

### MoviesApiDomain.csproj
```xml
<ItemGroup>
  <!-- Minimal - possibly just .NET base classes -->
  <!-- May include: System.ComponentModel.Annotations for data annotations -->
</ItemGroup>
```

### MoviesApiData.csproj
```xml
<ItemGroup>
  <ProjectReference Include="..\MoviesApiDomain\MoviesApiDomain.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.x" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.x" />
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.x" />
</ItemGroup>
```

### MoviesApi.csproj
```xml
<ItemGroup>
  <ProjectReference Include="..\MoviesApiDomain\MoviesApiDomain.csproj" />
  <ProjectReference Include="..\MoviesApiData\MoviesApiData.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.x" />
  <!-- Other API-specific packages -->
</ItemGroup>
```

---

## Migration Commands (Post-Separation)

All EF Core commands must target the Data project:

```bash
# Add new migration
dotnet ef migrations add MigrationName --project MoviesApiData --startup-project MoviesApi

# Update database
dotnet ef database update --project MoviesApiData --startup-project MoviesApi

# Remove migration
dotnet ef migrations remove --project MoviesApiData --startup-project MoviesApi

# Script migration
dotnet ef migrations script --project MoviesApiData --startup-project MoviesApi
```

**Important**: Run these commands from the `apps/web-api/` directory where the `.sln` file is located.

---

## Risks & Considerations

1. **Circular Dependencies**: Ensure MoviesApiData doesn't reference MoviesApi
2. **Service Layer**: Services stay in API layer but reference repositories from Data layer
3. **DTOs**: Keep in API layer as they're presentation concerns
4. **Testing**: Update test project to reference all three projects as needed
5. **Deployment**: Ensure database file path is correctly resolved from new location

---

## Success Criteria

- ✅ Solution builds without errors
- ✅ All unit tests pass
- ✅ API starts successfully
- ✅ All endpoints respond correctly
- ✅ Migrations can be added/applied from MoviesApiData project
- ✅ No circular dependencies between projects
- ✅ Clear separation of concerns maintained
- ✅ Controllers consolidated into single folder
- ✅ Test project references all required projects

---

## Quick Reference Checklist

### Before You Start
- [x] Backup current solution
- [x] Ensure all tests pass in current state
- [x] Note current EF Core/.NET versions from `MoviesApi.csproj`

### Project Creation ✅ COMPLETED
- [x] Create `MoviesApiDomain` class library
- [x] Create `MoviesApiData` class library
- [x] Add projects to solution
- [x] Set up project references

### Parallel Execution (Phase 2) ✅ COMPLETED
- [x] **Task 2A**: Domain layer setup (MoviesApiDomain) - 8 entities moved
- [x] **Task 2B**: Data layer setup (MoviesApiData) - DbContext, Migrations, Repositories moved
- [x] **Task 2C**: Test project update - References added

### Sequential Execution (Phase 3-6)
- [x] Phase 3: API layer update - All namespaces updated
- [x] Phase 4: Program.cs & DI configuration - Using statements updated
- [x] Phase 5: Build & integration test - ✅ **All 8 tests pass**
- [x] Phase 6: Cleanup & documentation - ✅ **COMPLETE**
  - [x] Removed empty folders (Models/, Data/, Repositories/, controllers/)
  - [x] Removed default Class1.cs files
  - [x] Updated README files for all projects
  - [x] Updated solution README with architecture overview

### File Moves
- [ ] Move Models → MoviesApiDomain
- [ ] Move DbContext, Migrations, Repositories → MoviesApiData
- [ ] Keep Controllers, DTOs, Services → MoviesApi
- [ ] Merge `controllers/` into `Controllers/`

### Namespace Updates
- [ ] Update Models namespace to `MoviesApiDomain.Models`
- [ ] Update Data namespace to `MoviesApiData.Data`
- [ ] Update Repositories namespace to `MoviesApiData.Repositories`
- [ ] Update all using statements across projects

### Package References
- [ ] Add EF Core packages to MoviesApiData
- [ ] Add project references to MoviesApi
- [ ] Remove redundant packages from MoviesApi

### Test Project Updates
- [ ] Add references to MoviesApiDomain and MoviesApiData
- [ ] Update using statements in test files
- [ ] Verify all tests compile
- [ ] Run all tests

### Final Steps
- [ ] Build solution
- [ ] Run API
- [ ] Test endpoints
- [ ] Remove empty folders
- [ ] Update documentation

# Changelog

All notable changes to the MoviesApp project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.2.0] - 2026-10-29

### 🎨 UI Mockup — Angular and MVC on the same design

The Angular client and the MVC app now render the **MoviesApp UI Mockup**:
the same 12-token OKLCH palette, the same sidebar shell, the same card grid
with coloured poster tiles, the same add / edit form and the same details
page. Both clients are pointed at the live Web API, and every app is held to
a 70% line-coverage gate in CI.

#### Added

##### Angular client (`apps/angular-client`)
- **92 component, store and service specs** covering the movies list, the
  add / edit form, the login page, the interceptors, the route guard, the
  lookup service and the reducers / selectors / effects.
- **`npm run test:coverage`** and the `@angular/build:unit-test` coverage
  configuration, with a 70% line threshold enforced by the builder itself.
- **`@vitest/coverage-v8`** — the collector the Angular unit-test builder
  needs to produce a coverage report.

##### MVC web app (`apps/mvc-web`)
- **The mockup design.** `wwwroot/css/site.css` was rewritten around the
  12 OKLCH design tokens; `Views/Shared/_Layout.cshtml` became the sidebar
  shell and `Views/Shared/_AuthLayout.cshtml` the centred sign-in card.
  Bootstrap and jQuery are gone.
- **Add and edit.** `MoviesController.Create` / `Edit`, the `MovieFormModel`
  view model, the `_MovieForm.cshtml` partial and the `Create.cshtml` /
  `Edit.cshtml` views. The form carries one id per lookup, and the
  Criterion number is only sent when the movie is a Criterion title.
- **A register page.** `AccountController.Register` and
  `Views/Account/Register.cshtml`, backed by the API's `RegisterDto`
  validation rules.
- **Server-side sign-out.** Signing out now revokes the refresh token on the
  API before the local session is cleared, so a token that stays valid
  server-side cannot be used to mint a new session.
- **A movie details page** in the mockup's style, and a filter row that
  carries the active search, genre, format, media type and sort state.
- **109 tests** — the API clients, the controllers, the session, the token
  handler and two `WebApplicationFactory` suites that boot the real pipeline
  and render the real Razor views.
- **`MovieTile`** — the poster tile's hash-derived colour and the title's
  initial, matching what the Angular client draws.

##### Web API (`apps/web-api`)
- **144 tests** — the contract, the repositories against a real SQLite
  database, the middleware, the token lifecycle and `WebApplicationFactory`
  integration suites that boot the API in memory.

##### CI
- **A 70% line-coverage gate per app**, enforced by
  `scripts/check-coverage.py` and published as a workflow artifact. The
  threshold lives in one place (`COVERAGE_THRESHOLD`) and the report format
  is detected from the file extension, so the Angular (Istanbul JSON) and
  the .NET (Cobertura XML) apps share one gate.

#### Fixed

##### Angular client (`apps/angular-client`)
- **`takeUntilDestroyed()` outside an injection context.** `ngOnInit` is not
  an injection context, so `MoviesListComponent` and `MovieFormComponent`
  threw `NG0203` at runtime and their subscriptions never tore down. Both
  components now hold an explicit `DestroyRef`.
- **The error interceptor's refresh-and-retry.** A failing refresh threw
  synchronously inside `catchError` instead of being routed to the same
  error path. The refresh is now deferred, so the interceptor clears the
  session and redirects to the login page exactly once.
- **The guard's argument list.** Angular 22's `CanMatchFn` takes three
  arguments.

##### MVC web app (`apps/mvc-web`)
- **The unmapped Razor Pages.** `Views/Privacy.cshtml` and
  `Views/Error.cshtml` were `@page` views in an app that only registers
  MVC controllers, so they were never reachable. They are now MVC views
  under `Views/Home/`, and the exception handler points at `/Home/Error`.
- **`HomeController` lived in the wrong namespace** (`MyApp.Namespace`), so
  it was never discovered by the MVC router.
- **The lookup routes.** `MoviesService` asked for `/api/v1/filmGenres`; the
  API's controller is `FilmGenresController`, so the route is
  `/api/v1/FilmGenres`.
- **The dead Bootstrap scoped CSS.** `Views/Shared/_Layout.cshtml.css` was
  left over from the Bootstrap shell. It matched nothing in the mockup and
  forced a scope attribute onto the layout's markup.

#### Removed

##### MVC web app (`apps/mvc-web`)
- **The dead legacy models** — `User`, `IUser`, `UserFavouriteMovies`,
  `IUserFavouriteMovies`, `CriterionTitle` and `ICriterionTitle`. They were
  left over from a standalone version of the app and are not referenced by
  any code path. The `User.Authenticate` guard test went with them: the
  method no longer exists, and the app never authenticates locally — it
  delegates to the API's Identity-backed auth endpoints.

## [4.1.0] - 2026-10-25

### 🔧 MVC Web App — Rebuilt Against the Real API Contract

The MVC app previously called routes the Web API does not expose, used `int`
ids where the API uses GUIDs, sent no credentials, and pointed at a
placeholder base URL. It could not talk to the API at all. It now does.

The API contract was verified by running the Web API and issuing real
requests — not inferred from the source. That process surfaced the route
casing (see below) and three API defects (recorded as G25–G27 in
`docs/PROJECT-STATUS.md`).

#### Added

##### MVC web app (`apps/mvc-web`)
- **`AccountController` + login page.** Signs in against `/api/v1/auth/login`,
  stores the access and refresh tokens in the ASP.NET Core session, and
  issues an `HttpOnly` `SameSite=Lax` auth cookie. `MovieSession` refreshes
  the short-lived access token automatically before it expires.
- **`ApiTokenHandler`.** A `DelegatingHandler` that reads the token from the
  session and attaches `Authorization: Bearer <token>` to every upstream
  request. Auth endpoints are excluded so a stale token cannot poison login.
- **`ApiAuthService`.** HTTP client for `/api/v1/auth` (login, register,
  refresh, logout).
- **`PagedResponse<T>`, `MediaTypeDto`, `FilmGenreDto`, `FormatDto`,**
  **`MovieCreateRequest`, `MovieUpdateRequest`** and the auth models —
  all mirroring the API's DTOs.
- **Server-rendered movie list** with search, genre / media-type / format
  filters, sorting and paging, backed by the API's paging metadata.
- **Movie details page** at `/Movies/MovieDetails/{guid}`.
- **`MovieListModel`** extended with the lookup lists and the active filter
  state.
- **32 tests** (`MoviesWeb.Tests`): `MoviesServiceTests`, `MoviesControllerTests`,
  `MovieQueryTests`. Up from 1.

#### Changed

##### MVC web app (`apps/mvc-web`)
- **`Movie.Id` is now a `Guid`**, matching the API's `MovieResponseDto`.
- **`Movie` mirrors the API's camelCase wire format.** The `Director` and
  `ReleaseDate` fields were removed — the API does not expose them. `Description`,
  `Favourite`, `Criterion` and `CriterionNumber` were added.
- **`MoviesService` was rewritten.** It targets `/api/v1/Movie` (singular) and
  `/api/v1/Movie/{guid}`, parses the paged envelope, and throws a descriptive
  `HttpRequestException` on failure. The old `movies/query` call — a route the
  API does not have — is gone.
- **`MovieQuery` allow-lists the API's eight real query keys** (`search`,
  `genre`, `mediaType`, `format`, `sortBy`, `sortDir`, `pageNumber`,
  `pageSize`) instead of the four keys the API never supported. Blank values
  are dropped so the API applies its own defaults.
- **`MoviesController` is `[Authorize]`** and renders the paged model. The
  dead `NotFound` branch and the client-side `fetch('/movies')` in the list
  view are gone; the view now renders server-side.
- **`Program.cs` validates `MoviesApi:BaseUrl`** at startup and throws if it
  does not include the `/api/v1` prefix.
- **Base URL defaults corrected** to `http://localhost:8081/api/v1` in
  `appsettings.json` and `appsettings.Development.json`, and
  `http://web-api:8080/api/v1` in `docker-compose.yml`.
- **Removed** the stale `Views/Movie/` folder, whose views linked to
  controller actions that do not exist.

#### Documentation

The documentation was audited against the running code and rewritten. Every
claim below was verified by executing the code, not by reading it.

- **`docs/PROJECT-STATUS.md`** rewritten against the running code (2026-10-25).
  Test counts, routes, JSON casing and paging shapes are all verified rather
  than assumed. Now carries **36 numbered gaps (G1-G36)** with per-item status.
- **`docs/ARCHITECTURE.md`** now documents the real API contract and carries a
  Mermaid diagram regenerated from the current component and service layout.
- **`docs/moviesapp-diagram.mmd`** regenerated — 26 labelled nodes, 33 edges,
  all three apps, SSR included. Every referenced source path was checked to
  exist.
- **`docs/CI-CD.md`** documents all four workflows, the path filters, and the
  known `NU1903` build warning.
- **`docs/SECURITY-OVERVIEW.md`** — the Angular and MVC sections now describe
  what the code actually does, including the open weaknesses.
- **`docs/SECRETS-MANAGEMENT.md`** — added the MVC app's token and cookie
  surfaces, which are new since 4.0.0.
- **`docs/k8s/PHASE-0-CHECKLIST.md`** — every task now carries a status, and
  four new sections (0.9-0.12) cover the defects found on 2026-10-25.
- **`apps/angular-client/README.md`** replaced; it was still the default
  Angular CLI scaffold text.
- **Route references corrected** across `docs/`, `apps/web-api/README.md`,
  `apps/web-api/MoviesApi/README.md` and the k8s manifests from
  `/api/v1/movies` to `/api/v1/Movie`.
- **Historical documents bannered** (`.NET 10 migration` set,
  `PROJECT-SEPARATION-PLAN.md`, both September search-sprint files) so they
  are not mistaken for current state.
- **All 148 local Markdown links across 42 files now resolve**; 25 broken
  links were fixed, including references to a `k8s/` directory that was never
  created and to an image asset that does not exist.

#### Tracked work

The remediation list is now tracked as 10 GitHub issues under the milestone
`4.2.0 - API contract and client correctness`:

| # | Issue | Priority |
| --- | --- | --- |
| [#14](https://github.com/andreak3779/MoviesApp/issues/14) | Angular: movie service calls `/api/v1/movies`, but the route is `/api/v1/Movie` | P0 |
| [#15](https://github.com/andreak3779/MoviesApp/issues/15) | Angular: models expect PascalCase, but the API emits camelCase | P0 |
| [#16](https://github.com/andreak3779/MoviesApp/issues/16) | Angular: access token is never refreshed, so sessions die after 15 minutes | P1 |
| [#17](https://github.com/andreak3779/MoviesApp/issues/17) | Angular: `errorInterceptor` clears `sessionStorage` but not the `AuthService` signal | P1 |
| [#18](https://github.com/andreak3779/MoviesApp/issues/18) | Web API: `POST /api/v1/Movie` returns 500 when a `formatId` is supplied | P0 |
| [#19](https://github.com/andreak3779/MoviesApp/issues/19) | Web API: `PUT /api/v1/Movie/{id}` silently drops most fields | P0 |
| [#20](https://github.com/andreak3779/MoviesApp/issues/20) | Web API: `GET /api/v1/Movie/{id}` returns empty `formats` | P1 |
| [#21](https://github.com/andreak3779/MoviesApp/issues/21) | Web API: add integration tests that boot the API with `WebApplicationFactory` | P1 |
| [#22](https://github.com/andreak3779/MoviesApp/issues/22) | Angular: paginator ignores the API's paging metadata | P2 |
| [#23](https://github.com/andreak3779/MoviesApp/issues/23) | Web API: `AuditLoggingMiddleware` is dead code — it runs before `UseAuthentication` | P1 |

#### Known issues (not fixed here)

Found while verifying the contract; tracked as G25–G28 and G36 in
`docs/PROJECT-STATUS.md`.

- **`POST /api/v1/Movie` returns 500 when a `formatId` is supplied** —
  `GetByIdAsync` omits `Include(MovieFormats)`.
- **`PUT /api/v1/Movie/{id}` silently drops** `Description`, `Favourite`,
  `Criterion`, `CriterionNumber` and the format collection —
  `MovieRepository.UpdateAsync` does not copy them.
- **`GET /api/v1/Movie/{id}` returns empty `formats`** — the ownership-checked
  query omits `Include(MovieFormats)`.
- **The Angular client still targets `/api/v1/movies`** and expects PascalCase
  while the API emits camelCase. It loads no data until both are corrected.
- **`AuthService.GenerateJwtToken` can emit a null `Name` claim** (CS8604) —
  the interpolated name is trimmed with `?.`, so it is `null` when both name
  parts are empty. The build still succeeds; the warning appears every time.
  Tracked as G36.

---

## [4.0.0] - 2026-09-26

### 🎬 MoviesApp UI — Mockup Implementation

Implements the four routes defined in `apps/angular-client/MoviesApp UI Mockups/MoviesApp.dc.html` end-to-end against the Web API.

#### Added

##### Web API (`apps/web-api`)
- **`Format` entity + `MovieFormat` junction table.** Mirrors `FilmGenre` / `MediaType`; full CRUD on `FormatsController`. Seed data: Movie / TVSeries / BoxSet.
- **POST / PUT / DELETE on `FilmGenresController` and `MediaTypesController`.** Anonymous reads, `UserOnly` writes, audit-logged.
- **`Movie` entity enriched.** New columns: `Description`, `Favourite`, `Criterion`, `CriterionNumber`. Defaults are safe (false / null) so existing rows remain valid.
- **Search / filter / sort / paging on `GET /api/v1/movies`.** New query params: `search`, `genre`, `mediaType`, `format`, `sortBy` (`title` | `year`), `sortDir` (`asc` | `desc`), `pageNumber`, `pageSize`. Ownership-scoped; reuses `PagedResponse<T>`.
- **`MovieResponseDto` carries the mockup's denormalised fields.** New: `Description`, `Favourite`, `Criterion`, `CriterionNumber`, `Genre`, `MediaType`, `Format` (strings). Full collections still returned for callers that want them.
- **`MovieCreateDto` / `MovieUpdateDto` accept single-select IDs** (`FilmGenreId`, `MediaTypeId`, `FormatId`). Legacy list-of-ids fields still accepted for backwards compatibility.
- **DELETE on `MovieController` now constrained to `:guid`** — matches the GET / PUT routes; removes the inconsistency where any string matched.

##### Angular client (`apps/angular-client`)
- **Folder move:** `src/models/` and `src/services/` → `src/app/models/` and `src/app/services/`, matching the convention in `AGENTS.md`, `.github/copilot-instructions.md`, and `docs/PROJECT-STATUS.md`.
- **HTTP `MovieService`** replaces the in-memory implementation. `search(query)`, `getById(id)`, `create(payload)`, `update(id, payload)`, `remove(id)`. Returns `Observable<…>` to match the existing NgRx effects.
- **`AuthService` + `auth.interceptor.ts` + `error.interceptor.ts` + `authGuard` (canMatch).** JWT + refresh token in `sessionStorage`. 401s redirect to `/login` with `returnUrl`. `/login` is the only route outside the auth guard.
- **`LookupService`** caches `filmGenres`, `mediaTypes`, `formats` as signals. `LookupAdminService` does the writes for the lookup screens.
- **Mockup-shaped UI:**
  - `MoviesListComponent` — filter row (search + three select filters + sort), card grid with criterion badges and favourite stars, paginator, delete-confirmation modal.
  - `MovieFormComponent` — Reactive Forms, single route used for both `/movies/new` and `/movies/:id/edit`, conditional `criterionNumber` validator, dispatches `addMovie` / `updateMovie`.
  - `LookupListComponent` and `LookupFormComponent` — parameterised by `LookupKind`, six routes total (Genres / Media Types / Formats × list / form).
  - `SidebarComponent` — left rail with Movies / Genres / Media Types / Formats nav, `RouterLinkActive` highlights, sign-out button.
  - `DeleteConfirmationComponent` — shared modal used by the movies list and lookup lists.
- **`Movie.Id` type tightened** from `number` to `string` (matches the API's `Guid`).

#### Fixed

- **`RateLimitingMiddleware` ctor now takes `IOptions<RateLimitOptions>`** instead of the raw POCO. Configured 100 / 10 / 60 values now actually bind — previously the middleware received a default-constructed instance and only happened to match the configured values by coincidence.
- **Pre-existing `app.spec.ts > should render title` assertion** rewritten to match the new shell layout (asserts on `.brand` text).

#### Changed

- **`Update Movie` action takes a full `Movie` payload** (it always did, but the type contract is now explicit).
- **`Load Movies` action takes a `MovieQuery` payload** so the component can pass search / filter / sort / page.

#### Compatibility notes

- The `FormatDto` is a new DTO; clients ignoring it continue to work.
- The denormalised `Genre`, `MediaType`, `Format` strings on `MovieResponseDto` are derived from the join tables server-side. They are accurate only for the **first** related entity per movie — multiple-genre movies will display only the first genre.
- Migration to be generated: `dotnet ef migrations add AddMovieEnrichmentAndFormats --project MoviesApiData --startup-project MoviesApi`. See `apps/web-api/MoviesApiData/Migrations/REGENERATE-NOTES.md` for the manual SQL fallback.

#### Test status

- Web API: **8 / 8** (`MovieControllerTests`)
- Angular: **26 / 26** (effects, reducer, selectors, app shell, services)

---

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
  - Automatic logging of all CREATE / UPDATE / DELETE operations
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
- `POST /api/v1/auth/login` - User login (returns JWT + refresh token)
- `POST /api/v1/auth/refresh` - Refresh access token
- `POST /api/v1/auth/logout` - Revoke refresh token
- `GET /api/v1/auth/me` - Get current user info
- `GET /api/v1/movies` - Paginated movie list
- `GET /api/v1/movies/{id}` - Movie by ID
- `POST /api/v1/movies` - Create movie
- `PUT /api/v1/movies/{id}` - Update movie
- `DELETE /api/v1/movies/{id}` - Delete movie
- `GET /api/v1/mediatypes` - List media types
- `GET /api/v1/filmgenres` - List film genres
- `GET /health` - Health check

#### Technical Details
- All 8 unit tests pass
- Docker containers build and run successfully
- EF Core 10 migrations execute correctly
- Swagger UI works with .NET 10
- No breaking changes to API contracts

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

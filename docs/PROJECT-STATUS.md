# MoviesApp — Project Status

**Last reviewed:** 2026-10-29
**Scope:** Whole monorepo (`apps/angular-client`, `apps/mvc-web`, `apps/web-api`)
**Branch:** `feat/api-swagger-angular-live-data-mvc-mockup`
**Audience:** Senior developers, reviewers, maintainers
**Format:** Snapshot of current state, gaps, risks, and recommendations.

> **How this document was verified.** Every claim below was checked against
> the running code, not inferred from reading it. The API contract was
> confirmed by starting the Web API and issuing real requests. The MVC
> rewrite was confirmed by running the API and the MVC app together and
> exercising register -> login -> list -> details -> filter -> delete.
> Test counts and coverage numbers are the real output of
> `npm run test:coverage` and `dotnet test` (both .NET solutions) as of the
> date above.

## 1. Executive Summary

MoviesApp is a three-app monorepo for a personal movie collection. The
Angular client and the MVC web app both consume a single OWASP-hardened Web
API. The repository layout, CI/CD pipeline, Dockerfiles, and documentation
are mature. **All three apps now build, all three test suites are green,
and all three point at the live API contract.** Each app is held to a 70%
line-coverage gate in CI. The remaining work is the three genuine API
defects recorded below and the feature gaps in the original brief.

| Area | Status | Confidence |
| --- | --- | --- |
| Repository scaffolding, monorepo hygiene, CI/CD | ✅ Solid | High |
| Web API (`apps/web-api`) | ✅ Strong security baseline; **3 known defects** | High |
| MVC web app (`apps/mvc-web`) | ✅ On the live contract and on the UI mockup | High |
| Angular client (`apps/angular-client`) | ✅ On the live contract and on the UI mockup | High |
| Container / local orchestration | ✅ Working | High |
| Tests | ✅ 345 total, all passing; 70% coverage gate per app | High |
| Documentation | ✅ Refreshed 2026-10-29 | High |

**Test totals (verified):** Angular 92/92, Web API 144/144, MVC 109/109 —
**345 passing, 0 failing.**

**Line coverage (verified):** Angular 79.07%, Web API 90.59%, MVC 93.42% —
all above the 70% gate in `scripts/check-coverage.py`.

### What changed since the previous review (2026-09-05)

The previous version of this document was seven weeks stale and described the
Angular app as a skeleton and the MVC app as a placeholder. Both were wrong by
then. The substantive changes are:

1. **The Angular client was completed** (release 4.0.0). It now has an HTTP
   data layer, JWT auth with an interceptor and a route guard, an NgRx slice
   with actions/reducer/selectors/effects, and six mockup-shaped routes.
2. **The MVC app was rewritten** (release 4.1.0, this branch). It previously
   called routes the API does not have, used `int` ids where the API uses
   GUIDs, sent no credentials, and pointed at a placeholder base URL. All of
   that is fixed and covered by 32 tests plus a live end-to-end run.
3. **The API route is `/api/v1/Movie`, not `/api/v1/movies`** — the controller
   is named `MovieController`. This was found by running the API, and it
   affects the Angular client as well (see G28).
4. **Three API defects were found** during that verification and are recorded
   as G25-G27 below. Two of them cause silent data loss.

## 2. Repository Topology

```
MoviesApp/
├── apps/
│   ├── angular-client/    Angular 22 client (standalone + SSR + NgRx 22)
│   ├── mvc-web/           ASP.NET Core MVC (server-rendered, Razor)
│   └── web-api/           ASP.NET Core Web API (Clean Architecture: Domain → Data → Api)
├── docs/                  Architecture, CI/CD, security, migration plans
├── .github/               Agent guidance + filters + workflows
└── docker-compose.yml     Local full-stack
```

### Per‑app directories of note

**Angular client** (`apps/angular-client/src/app/`)

- `core/` — `api-config.ts` (the `API_BASE_URL` injection token),
  `auth/login.component.ts`, `guards/auth.guard.ts`,
  `interceptors/auth.interceptor.ts`, `interceptors/error.interceptor.ts`,
  `services/auth.service.ts`, `services/lookup.service.ts`.
- `features/` — `chrome/sidebar.component.ts`,
  `movies/movies-list.component.ts`, `movies/movie-form.component.ts`,
  `lookups/lookup-list.component.ts`, `lookups/lookup-form.component.ts`,
  `lookups/lookup.service.ts` (`LookupAdminService`),
  `shared/delete-confirmation.component.ts`.
- `models/` — `movie.ts`, `film-genre.ts`, `media-type.ts`, `format.ts`.
- `services/` — `movie-service.ts`.
- `store/movies/` — the NgRx feature slice: actions, reducer, selectors, effects and their specs.
- `mockoon/movie-api.json` — Mockoon environment; `scripts/mock-server.js` provides a Node fallback used by CI.

> Note the two similarly-named lookup services: `core/services/lookup.service.ts`
> is the **read-only cached** service used by the list and form components;
> `features/lookups/lookup.service.ts` is `LookupAdminService`, the **write**
> client. They are easy to confuse.

**Web API** (`apps/web-api/`)

- `MoviesApiDomain/Models/` — POCO entities: `Movie` (GUID), `MediaType`,
  `FilmGenre`, `Format`, the three junction tables, `MovieQuery`,
  `ApplicationUser`, `RefreshToken`, `AuditLog`.
- `MoviesApiData/` — `MoviesDbContext` + EF Core migrations + repositories.
- `MoviesApi/` — controllers, DTOs, services (`AuthService`, `AuditLogService`,
  `SensitiveFieldRedactor`), the four middlewares, `Program.cs`.
- `MoviesApi.Tests/` — xUnit tests for the controller, the secret-key guard,
  and the redactor.

**MVC web app** (`apps/mvc-web/MoviesWeb/`)

- `Controllers/` — `HomeController`, `MoviesController`, `AccountController`.
- `Services/` — `MoviesService`, `IMoviesService`, `ApiAuthService`,
  `IApiAuthService`, `MovieSession`, `ApiTokenHandler`.
- `Models/` — `Movie` / `IMovie`, `MovieQuery`, `PagedResponse<T>`,
  the three lookup DTOs, the auth models, the create / update payloads.
- `ViewModels/` — `MovieListModel`, `MovieFormModel`, `MovieTile`,
  `ErrorViewModel`.
- `Views/` — `Home` (Index, Privacy, Error), `Movies` (Index, Create, Edit,
  `_MovieForm`, MovieDetails), `Account` (Login, Register), `Shared`
  (`_Layout`, `_AuthLayout`).

---

## 3. Current Implementation Snapshot

### 3.1 Angular client (`apps/angular-client`)

| Item | State |
| --- | --- |
| Framework | Angular **22.2** (standalone components), NgRx **22**, TypeScript 6.0, Vitest 5 |
| Styling | SCSS. Angular Material 22 is installed but no theme is wired up. |
| Forms | Reactive Forms (`MovieFormComponent`, `LookupFormComponent`, `LoginComponent`) |
| State | NgRx `store`, `effects`, `entity`, `router-store`, `store-devtools`. One feature slice: `movies`. |
| Routing | 19 route entries; 6 lazy-loaded feature routes (movies list/new/edit, lookup list/new × 3 kinds), `login` outside the guard, `**` fallback to `/movies`. SSR uses `RenderMode.Prerender`. |
| Auth | `AuthService` + `authInterceptor` + `errorInterceptor` + `authGuard` (`canMatch`). Tokens in `sessionStorage`. |
| Data layer | `MovieService` and `LookupAdminService` are HTTP clients against `/api/v1`. `LookupService` caches the three lookup lists as signals. |
| Components | `App` (shell), `SidebarComponent`, `LoginComponent`, `MoviesListComponent`, `MovieFormComponent`, `LookupListComponent`, `LookupFormComponent`, `DeleteConfirmationComponent` |
| Tests | 14 spec files, **92 / 92** green (`app`, the movies list and form components, the reducers, selectors and effects, the interceptors, the guard, the login page, the lookup service and the HTTP error helper) |
| Server | `src/server.ts` (Express) + `src/main.server.ts`, `provideServerRendering(withRoutes(serverRoutes))` |
| Coverage | **79.07%** lines, gated at 70% in CI (`npm run test:coverage`) |

> **Verdict:** The client is feature-complete against the mockup and now
> **loads movies from the live API**. G28 (the plural `/api/v1/movies`
> route) and G29 (PascalCase expectations) are fixed: the service targets
> the singular `/api/v1/Movie` and the models are camelCase. The access
> token is refreshed by the error interceptor, so a session survives past
> the 15-minute token lifetime.

### 3.2 Web API (`apps/web-api`)

| Layer | Notes |
| --- | --- |
| Target framework | `net10.0` |
| Clean architecture | Domain (POCOs) -> Data (DbContext + Repositories + Migrations) -> Api (Controllers, DTOs, Services, Middleware). Domain has zero project dependencies. |
| Persistence | EF Core 10 + SQLite. `EnsureCreated()` on startup (not `Migrate()`), seed data for `MediaType`, `FilmGenre`, `Format`. |
| Migrations | One `InitialSchema` migration. History was deliberately reset; see `Migrations/REGENERATE-NOTES.md`. |
| Auth | ASP.NET Core Identity + JWT (HS256) + refresh tokens stored in `RefreshToken`, bound to the access token's `jti`, single-use. |
| Authorization | `[Authorize]` policies: `UserOnly` (default), `AdminOnly`. |
| Object-level ownership | `Movie.OwnerId`; every movie endpoint filters by the caller's claim. |
| Cross-cutting middleware | `SecurityHeaders`, `ExceptionHandling`, `RateLimiting`, `AuditLogging`. |
| Rate limiting | 100 req/min read, 10 req/min write, keyed by user id or IP, in-memory. |
| Audit logging | `AuditLogService` writes CREATE/UPDATE/DELETE rows; emitted from the controllers. Sensitive fields are redacted before persistence. |
| Security headers | CSP, HSTS (prod), X-Frame-Options DENY, X-Content-Type-Options nosniff, Permissions-Policy. |
| Versioning | URL path prefix `/api/v1`. Swagger enabled in Development only. |
| Tests | **144 / 144** — the contract, repository, middleware, secret-key guard, sensitive-field redactor and `WebApplicationFactory` integration suites |
| Coverage | **90.59%** lines, gated at 70% in CI |

**Endpoints (verified against a running instance):**

| Method | Route | Auth | Notes |
| --- | --- | --- | --- |
| POST | `/api/v1/auth/register` | anonymous | Returns tokens on success |
| POST | `/api/v1/auth/login` | anonymous | |
| POST | `/api/v1/auth/refresh` | anonymous | Refresh token is single-use |
| POST | `/api/v1/auth/logout` | authorize | Revokes the caller's own refresh token |
| GET | `/api/v1/auth/me` | authorize | |
| GET | `/api/v1/Movie` | `UserOnly` | Paged, ownership-scoped, searchable/filterable/sortable |
| GET | `/api/v1/Movie/{id:guid}` | `UserOnly` | Ownership-checked |
| POST | `/api/v1/Movie` | `UserOnly` | Audit logged — **500 when `formatId` is set** (G25) |
| PUT | `/api/v1/Movie/{id:guid}` | `UserOnly` | Audit logged — **silently drops fields** (G26) |
| DELETE | `/api/v1/Movie/{id:guid}` | `UserOnly` | Audit logged |
| GET | `/api/v1/filmgenres`, `/mediatypes`, `/formats` | anonymous | Lookup lists, also `GET /{id}` |
| GET | `/health` | anonymous | Docker healthcheck |

> **Verdict:** The strongest part of the repository. The OWASP baseline is
> real — implemented in code, migrations, and tests. The three defects in
> G25-G27 are the main outstanding risk.

### 3.3 MVC web app (`apps/mvc-web`)

Rebuilt in 4.1.0, put on the UI mockup in 4.2.0. See
[apps/mvc-web/README.md](../apps/mvc-web/README.md).

| Item | State |
| --- | --- |
| Target framework | `net10.0` |
| Design | The **MoviesApp UI Mockup**: OKLCH design tokens in `wwwroot/css/site.css`, a 220px sidebar shell, a card grid with coloured poster tiles, and a centred sign-in card. No Bootstrap, no jQuery. |
| Razor views | `Views/Home` (Index, Privacy, Error), `Views/Movies` (Index, Create, Edit, `_MovieForm`, MovieDetails), `Views/Account` (Login, Register), `Views/Shared` (`_Layout`, `_AuthLayout`). All server-rendered. |
| Controllers | `HomeController`, `MoviesController` (`[Authorize]`; list, add, edit, details, delete), `AccountController` (sign in, register, sign out). |
| Auth | `AccountController` calls `/api/v1/Auth/login` and `/register`; the JWT and refresh token are stored in the ASP.NET Core session by `MovieSession`; `ApiTokenHandler` attaches `Authorization: Bearer <token>` to every upstream request. The access token is refreshed automatically before it expires, and signing out **revokes the refresh token on the API** before the local session is cleared. |
| Services | `MoviesService` (`/api/v1/Movie` plus the three lookups), `ApiAuthService` (`/api/v1/Auth`), both `AddHttpClient`-registered. |
| Models | `Movie` (Guid id, camelCase properties matching `MovieResponseDto`), `MovieQuery` (allow-listed to the API's eight keys), `PagedResponse<T>`, `MediaTypeDto` / `FilmGenreDto` / `FormatDto`, auth models, create/update payloads. The dead legacy models (`User`, `IUser`, `UserFavouriteMovies`, `IUserFavouriteMovies`, `CriterionTitle`, `ICriterionTitle`) were removed. |
| ViewModels | `MovieListModel` (paging metadata and filter state), `MovieFormModel` (the add / edit form), `MovieTile` (the poster tile's colour and letter), `ErrorViewModel`. |
| Tests | **109 / 109** — `MoviesServiceTests`, `MoviesControllerTests`, `MovieQueryTests`, `ApiAuthServiceTests`, `MovieSessionTests`, `ApiTokenHandlerTests`, `AccountControllerTests`, `MovieTileTests`, `HomeControllerTests`, and two `WebApplicationFactory` page suites that render the real Razor views. |
| Coverage | **93.42%** lines, gated at 70% in CI (`MoviesWeb.Tests/coverlet.runsettings`). |
| Docker | SDK 10.0 -> ASP.NET 10.0, port 8080, non-root user, `/health` healthcheck. |
| Compose wiring | `mvc-web` depends on `web-api` (healthy); calls the API via `MoviesApi__BaseUrl`, which must include the `/api/v1` prefix. `Program.cs` refuses to start otherwise. |

> **Verdict:** The MVC app speaks the real API contract and renders the
> mockup design. It was verified end-to-end against a running API
> (register -> sign in -> list -> details -> filter -> sign out), and the
> pages are now covered by integration tests that boot the real pipeline
> and render the real views.

## 4. Architecture Findings

### 4.1 Strengths

- **Monorepo with per‑app CI gating** — change detection in `.github/filters.yaml` scopes workflows per app, so docs edits do not waste Docker minutes.
- **Web API clean architecture** — Domain has zero dependencies; Data depends on Domain; Api composes both. This is rare to see actually enforced.
- **OWASP implementation is genuine** — JWT with refresh‑token rotation, GUID resource IDs, ownership filtering at the repository, per‑user rate limiting, audit logging of CREATE/UPDATE/DELETE, secure headers middleware, generic error responses, Swagger disabled in production.
- **Angular NgRx setup is idiomatic** — `createActionGroup`, `EntityAdapter`, `createEffect`, mock‑actions in tests. The store is wired with `provideRouterStore` so route state is replayable.
- **SSR is correct** — `provideServerRendering(withRoutes(serverRoutes))` and a working Express handler with health endpoint.
- **Docker is clean** — multi‑stage builds with explicit runtime images, non‑root‑friendly (no root user, but worth pinning explicitly later), healthchecks on every service.

### 4.2 Gaps and Inconsistencies

| # | Gap | Where | Status / Impact |
| --- | --- | --- | --- |
| **G25** | **`POST /api/v1/Movie` returns 500 when a `formatId` is supplied.** `CreateAsync` -> `GetByIdAsync` omits `Include(MovieFormats)`, so `MapToResponseDto` dereferences a null `Format` and throws `NullReferenceException` | `MoviesApiData/Repositories/MovieRepository.cs` (`GetByIdAsync`) | **Fixed (2026-10-27) — P0.** `GetByIdAsync` now includes `MovieFormats` with `ThenInclude(Format)`. Guarded by `MovieRepositoryTests.CreateAsync_WithFormat_ReturnsMovieWithFormatHydrated`. |
| **G26** | **`PUT /api/v1/Movie/{id}` silently drops most fields.** `UpdateAsync` copies only `Title`, `ReleaseYear`, `MovieMediaTypes`, `MovieFilmGenres` — `Description`, `Favourite`, `Criterion`, `CriterionNumber`, `UpdatedAt` and the whole `MovieFormats` collection are discarded | `MoviesApiData/Repositories/MovieRepository.cs` (`UpdateAsync`) | **Fixed (2026-10-27) — P0.** `UpdateAsync` now copies every scalar and diffs each junction table by FK id, reading the current set from the database rather than from a tracked collection. The controller builds a detached desired-state document and carries the complete desired link set for every collection. Guarded by three `MovieRepositoryTests` cases. |
| **G27** | **`GET /api/v1/Movie/{id}` returns empty `formats`.** `GetByIdWithOwnershipCheckAsync` omits `Include(MovieFormats)`, so `format` is null and `formats` is `[]` even when the movie has one | `MoviesApiData/Repositories/MovieRepository.cs` | **Fixed (2026-10-27) — P1.** `GetByIdWithOwnershipCheckAsync` now includes `MovieFormats` with `ThenInclude(Format)`. Guarded by `MovieRepositoryTests.GetByIdWithOwnershipCheckAsync_ReturnsFormatsHydrated`. |
| **G28** | **The Angular client calls `/api/v1/movies`, which is a 404.** The route is the singular `/api/v1/Movie` because the controller is `MovieController`. All five `MovieService` methods are affected. The lookup routes (`/filmGenres`, `/mediaTypes`, `/formats`) are correct — only the movie routes are wrong | `apps/angular-client/src/app/services/movie-service.ts` | **Fixed — P0.** Found by running the API. The MVC and Angular clients now call `/api/v1/Movie`. |
| **G29** | **The Angular client expects PascalCase JSON; the API emits camelCase.** `Movie.Id`, `Movie.Title`, `Movie.ReleaseYear` and `PagedResponse.items` never bind, so the entity adapter's `selectId` returns `undefined` and the grid stays empty even once the route is fixed. Note the lookup DTOs (`FilmGenreDto.id`, `.name`) are already camelCase and *do* bind | `apps/angular-client/src/app/models/movie.ts` | **Fixed — P0.** Same root cause as G28: the client was written against a contract that was never observed. The Angular models now use the API's camelCase. |
| **G30** | **The Angular access token is never refreshed.** `AuthService.refresh()` exists but nothing calls it, and no interceptor retries a 401 with the refresh token | `apps/angular-client/src/app/core/` | **Open — P1.** Sessions end after 15 minutes. The MVC client already solves this via `MovieSession`. |
| **G31** | **`errorInterceptor` clears `sessionStorage` directly instead of calling `AuthService.clear()`,** so the in-memory token signal stays set and `authGuard.isAuthenticated()` still returns true after a 401 | `apps/angular-client/src/app/core/interceptors/error.interceptor.ts` | **Open — P1.** The guard will not bounce the user to `/login`. |
| **G15** | Angular has no token-refresh strategy (superseded and narrowed by G30) | `apps/angular-client/src/app/core/services/auth.service.ts` | Open — see G30. |
| **G7** | Rate limiting is in-memory (`ConcurrentDictionary`) — does not survive a restart or scale across instances | `apps/web-api/MoviesApi/Middleware/RateLimitingMiddleware.cs` | Open — fine for a single instance; needs a distributed cache for HA. |
| **G9** | `AuthService.GenerateJwtToken()` calls `_userManager.GetRolesAsync(user).Result` — a blocking `.Result` inside a non-async method | `apps/web-api/MoviesApi/Services/AuthService.cs` | Open — thread-pool starvation risk under load. |
| **G10** | `AuthService.RefreshTokenAsync` catches every exception and returns `null` with no logging | `apps/web-api/MoviesApi/Services/AuthService.cs` | Open — silent failures in the auth flow. |
| **G12** | The audit middleware and the controllers both log the same mutation, so each mutation is recorded twice | `AuditLoggingMiddleware.cs` vs the controllers | Open — noisy audit table. |
| **G32** | `AuditLoggingMiddleware` is registered before `UseAuthentication`, so `context.User` is unauthenticated when it reads the claim — its own audit branch never fires and only the controller-written rows are stored | `apps/web-api/MoviesApi/Program.cs` (middleware order) | Open — the middleware is effectively dead code. |
| **G11** | `MovieController.Delete` uses `{id:guid}` while the changelog still says it was unconstrained — **now consistent**; the old note was stale | `apps/web-api/MoviesApi/Controllers/MovieController.cs` | Resolved in 4.0.0. |
| **G8** | ~~Rate limit options not registered in DI~~ — **fixed in 4.0.0**; `RateLimitingMiddleware` now takes `IOptions<RateLimitOptions>` and the configured 100/10 values bind correctly | `apps/web-api/MoviesApi/Middleware/RateLimitingMiddleware.cs` | Resolved. |
| **G16** | ~~No query/filter endpoint~~ — **fixed in 4.0.0**; `GET /api/v1/Movie` supports search, genre, media type, format, sort and paging | `apps/web-api/MoviesApi/Controllers/MovieController.cs` | Resolved. Filtering by criterion number and favourite is still not supported. |
| **G17** | The watch list from the brief has no model, service, controller, endpoint, store slice, or component anywhere | All apps | Open — the largest remaining gap against the brief. |
| **G33** | The lookup controllers bind and return domain entities (`FilmGenre`, `MediaType`, `Format`) rather than DTOs, unlike `MovieController` | `apps/web-api/MoviesApi/Controllers/FilmGenresController.cs`, `MediaTypesController.cs`, `FormatsController.cs` | Open — inconsistent contract and a mass-assignment surface. |
| **G21** | `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 and `AutoMapper` 16.2.0 had known high-severity advisories | `apps/web-api/MoviesApiData/MoviesApiData.csproj` | **Fixed (2026-11-18).** `AutoMapper` is 16.3.0; `dotnet list package --vulnerable --include-transitive` reports no vulnerable packages. |
| **G20** | The Mockoon config and `scripts/mock-server.js` still describe the old flat `/movies` contract with integer ids | `apps/angular-client/mockoon/movie-api.json`, `apps/angular-client/scripts/mock-server.js` | Open — CI starts the mock but the Angular tests inject a stubbed service, so it is no longer a faithful contract source. |
| **G35** | A large commented-out `app.MapGet` endpoint block remains at the bottom of the API's `Program.cs` | `apps/web-api/MoviesApi/Program.cs` | Minor — cleanup. |
| **G34** | The Angular paginator derives `hasMore` from `movies().length >= pageSize` and ignores the API's `hasNext` / `totalCount`; the NgRx effect also discards the paging metadata | `apps/angular-client/src/app/features/movies/movies-list.component.ts`, `store/movies/movies.effects.ts` | Open — the last page still shows a live "Next" button. |
| **G18** | No startup probe for the Angular SSR container | `apps/angular-client/Dockerfile` | Minor. |
| **G19** | No `manifest.webmanifest`; PWA artefacts absent | `apps/angular-client/src/index.html` | Minor. |
| **G22** | The Web API README and several docs still describe the old `/api/v1/movies` route and integer ids | `apps/web-api/README.md`, `docs/` | **Resolved 2026-10-25** — routes corrected across the docs. |
| **G36** | **`AuthService.GenerateJwtToken` can emit a null `Name` claim.** The interpolated first/last name is trimmed with `?.`, so it yields `null` when both name parts are empty, and `new Claim(ClaimTypes.Name, null)` is a nullable-reference violation (CS8604). The API still builds — the warning appears on every build — but the claim is invalid, and a strict token validator could reject it | `apps/web-api/MoviesApi/Services/AuthService.cs` (line 250) | **Open — P2.** Confirmed by the build output on 2026-10-25. Fix by coalescing to `string.Empty`. |

### 4.3 Security Notes

**Strengths**

- ASP.NET Core Identity + JWT + single-use refresh tokens bound to the access token's `jti`.
- Object-level ownership on every movie read and write.
- Security headers, a generic error contract (RFC 7807 `ProblemDetails` with a `traceId`), and no stack traces echoed to clients.
- Audit logging with a `SensitiveFieldRedactor` that strips password/token-shaped fields before anything is persisted, and drops bodies entirely on the auth routes.
- The API refuses to start on the placeholder JWT secret or a secret shorter than 32 bytes — in `Program.cs` **and** in `AuthService` — so a misconfigured deploy fails fast rather than running insecurely.
- CORS origins come from configuration, and in Production the API refuses to start if the list still contains a placeholder origin.
- The MVC app validates that `MoviesApi:BaseUrl` includes the `/api/v1` prefix before serving.

**To address**

- The API's `Program.cs` still contains a large commented-out `app.MapGet` block at the end (G35 — cleanup).
- `RequireNonAlphanumeric = true` for passwords is opinionated and reduces accessibility; consider making it configurable.
- Refresh-token reuse detection is partial: a revoked token is refused, but presenting one does not cascade-revoke the token family (OWASP recommends that).
- The audit middleware swallows JSON parse errors silently (acceptable, but a separate failure log would help).
- Rate limiting is per-process, so it does not hold across replicas (G7).

## 5. Testing Coverage Snapshot

All three suites were run on 2026-10-29. **345 tests, all passing.** Each app
is held to a **70% line-coverage gate** in CI by
`scripts/check-coverage.py`; the report is published as a workflow artifact.

| App | Tests | Result | Line coverage | What is covered |
| --- | --- | --- | --- | --- |
| Angular | 92 | 92 / 92 | **79.07%** | The movies list and form components, the reducers, selectors and effects, the auth and error interceptors, the route guard, the login page, the lookup service and the HTTP error helper. |
| Web API | 144 | 144 / 144 | **90.59%** | The API contract, the repositories against a real SQLite database, the middleware, the secret-key guard, the sensitive-field redactor and the token lifecycle, plus `WebApplicationFactory` integration suites that boot the API in memory. |
| MVC | 109 | 109 / 109 | **93.42%** | `MoviesService` and `ApiAuthService` (route, method, body and envelope shape against a mocked handler), the controllers (list, add, edit, details, delete, sign in, register, sign out), `MovieSession`, `ApiTokenHandler`, `MovieQuery`, and two `WebApplicationFactory` page suites that render the real Razor views. |

### Gaps in coverage

| Area | Status |
| --- | --- |
| Angular component tests | **Partly covered.** `MoviesListComponent`, `MovieFormComponent`, the interceptors, the guard and the login page have specs. The lookup list and lookup form components still have none. |
| Web API middleware | **Covered.** The middleware, the repositories and the token lifecycle all have tests; the integration suites boot the real API in memory. |
| Web API repositories | **Covered.** `MovieRepositoryTests` (2026-10-27) exercises `GetByIdAsync`, `GetByIdWithOwnershipCheckAsync`, `CreateAsync` and `UpdateAsync` against a real SQLite database, and would have caught G25-G27. |
| MVC integration | **Covered.** `MoviesPageTests` and `AccountPageTests` boot the real pipeline with `WebApplicationFactory` and render the real views. |
| E2E / browser tests | **None** in any app. |

> **Recommendation.** The remaining gap is end-to-end coverage across all
> three apps at once, and a browser-level check of the mockup design. Both
> need infrastructure the repository does not have yet.

## 6. CI/CD Status

| Workflow | Trigger | Behaviour |
| --- | --- | --- |
| `validate-apps.yml` | PR, schedule, manual | Per-app restore/build/test. Angular runs the mock server before `npm test` and `npm run build`; MVC and Web API run `dotnet build` + `dotnet test` in Release. Concurrency group `validate-apps-${{ github.ref }}` with cancel-in-progress. |
| `container-images.yml` | Manual + scheduled full | Per-app Docker build (no push). |
| `release-images.yml` | Manual + `release-*` tag push | Per-app publish to GHCR with deterministic tag naming. |
| `security.yml` | Every push | Secret scanning across the whole repo (the `security` filter in `.github/filters.yaml` matches `**`, ignoring `docs/**`). |

Change detection is centralised in `.github/filters.yaml`. A change under one
app folder runs only that app's job; a change to a shared file (workflows,
`docker-compose.yml`, `README.md`, `AGENTS.md`, `docs/**`, `.env.example`,
`.gitleaks.toml`, `.gitignore`, `.pre-commit-config.yaml`) runs all three.

Suggested branch-protection checks: `angular-client`, `mvc-web`, `web-api`,
`build-angular-client`, `build-mvc-web`, `build-web-api`.

**Follow-ups**

- Pin `actions/*` and `dorny/paths-filter` by SHA for high-value workflows.
- The release workflow does not use `cancel-in-progress`; the other three do.
- No Dependabot config for npm or NuGet.

## 7. Risks and Recommendations (Prioritised)

### P0 — correctness blockers

1. **Fix `MovieRepository.UpdateAsync` (G26).** It silently discards
   `Description`, `Favourite`, `Criterion`, `CriterionNumber`, `UpdatedAt` and
   the format collection. The controller dutifully sets those fields on the
   tracked entity before calling the repository, so the loss is invisible at
   the call site.
2. **Fix `MovieRepository.GetByIdAsync` (G25).** Creating a movie with a
   format returns a 500 `NullReferenceException`.
3. **Fix the Angular client's route and casing (G28, G29).** The client calls
   `/api/v1/movies` (404) and expects PascalCase while the API emits
   camelCase. Until both are corrected the Angular list and detail screens
   cannot load a single movie. The MVC client already does this correctly and
   is the reference implementation.

### P1 — feature completeness vs the brief

1. **Implement the watch list (G17).** No model, endpoint, store slice, or
   component exists. This is the largest gap against `MoviesApp Project.md`.
2. **Wire the Angular token refresh (G30, G31).** Sessions currently end
   after 15 minutes, and the 401 handler does not clear the in-memory token.
   `MovieSession` in the MVC app is a working reference.
3. ~~**Add repository tests to the Web API (G25-G27).**~~ **Done (2026-10-27).**
   `MovieRepositoryTests` covers the three defects that have now been fixed.

### P1 — security hardening

1. **Move `AuditLoggingMiddleware` after `UseAuthentication` (G32)** so it
   can read the caller's identity, or drop the middleware and keep the
   controller-side logging as the single source of truth (G12).
2. **Remove the `.Result` call in `AuthService.GenerateJwtToken` (G9)** and
   log the exceptions swallowed by `RefreshTokenAsync` (G10).
3. **Move to a distributed rate limiter (G7)** before running more than one
   API replica.
4. **Implement refresh-token reuse detection** — presenting a consumed token
    should cascade-revoke the family.

### P2 — hygiene

 1. **Add Angular component tests** for the list, form, interceptors, and guard.
 2. **Add an API integration test** using `WebApplicationFactory` that covers
    register -> login -> CRUD with ownership.
 3. **Add a middleware test suite** for the four Web API middlewares.
 4. ~~Bump `SQLitePCLRaw.lib.e_sqlite3`~~ Done 2026-11-18 (G21): no vulnerable
    packages remain.
 5. **Delete the commented-out endpoint block** at the bottom of the API's
    `Program.cs` (G35).
 6. **Move the lookup controllers to DTOs** so they match `MovieController`
    (G33).
 7. **Refresh or retire the Mockoon config** (G20) so the mock matches the
    real contract.
 8. **Use the API's paging metadata in the Angular paginator** (G34).
 9. **Enable Dependabot** for npm and NuGet; pin GitHub Actions by SHA.
10. **Document the required environment variables per image**
    (`JwtSettings__*`, `MoviesApi__BaseUrl`, `Cors__AllowedOrigins__*`) in
    `docs/`.
11. **Coalesce the null `Name` claim in `AuthService.GenerateJwtToken`
    (G36)** to clear the CS8604 warning that appears on every build.

## 7b. Tracked Work — GitHub Issues

The remediation list has been turned into **10 GitHub issues** under the
milestone **`4.2.0 - API contract and client correctness`**. Close them as you
work through them; this document's gap table stays the source of truth for the
*analysis*, the issues are where the *work* happens.

**Make the Angular UI load live data** (it currently loads nothing):

| # | Issue | Priority | Gap |
| --- | --- | --- | --- |
| [#14](https://github.com/andreak3779/MoviesApp/issues/14) | Angular: movie service calls `/api/v1/movies`, but the route is `/api/v1/Movie` | P0 | G28 |
| [#15](https://github.com/andreak3779/MoviesApp/issues/15) | Angular: models expect PascalCase, but the API emits camelCase | P0 | G29 |
| [#16](https://github.com/andreak3779/MoviesApp/issues/16) | Angular: access token is never refreshed, so sessions die after 15 minutes | P1 | G30 |
| [#17](https://github.com/andreak3779/MoviesApp/issues/17) | Angular: `errorInterceptor` clears `sessionStorage` but not the `AuthService` signal | P1 | G31 |

**Fix the API defects** (all in `MovieRepository`):

| # | Issue | Priority | Gap |
| --- | --- | --- | --- |
| [#18](https://github.com/andreak3779/MoviesApp/issues/18) | Web API: `POST /api/v1/Movie` returns 500 when a `formatId` is supplied | P0 | G25 |
| [#19](https://github.com/andreak3779/MoviesApp/issues/19) | Web API: `PUT /api/v1/Movie/{id}` silently drops most fields | P0 | G26 |
| [#20](https://github.com/andreak3779/MoviesApp/issues/20) | Web API: `GET /api/v1/Movie/{id}` returns empty `formats` | P1 | G27 |

**Tests and hardening:**

| # | Issue | Priority | Gap |
| --- | --- | --- | --- |
| [#21](https://github.com/andreak3779/MoviesApp/issues/21) | Web API: add integration tests that boot the API with `WebApplicationFactory` | P1 | — |
| [#22](https://github.com/andreak3779/MoviesApp/issues/22) | Angular: paginator ignores the API's paging metadata | P2 | G34 |
| [#23](https://github.com/andreak3779/MoviesApp/issues/23) | Web API: `AuditLoggingMiddleware` is dead code — it runs before `UseAuthentication` | P1 | G32, G12 |

### Suggested order

1. **#14 + #15 together.** Neither works alone — the route must be right *and*
   the casing must be right, or the grid stays empty.
2. **#18 + #19 + #20 as one pass.** Same file, same class of bug, one test
   file. Doing them separately means three rounds of the same setup.
3. **#21** — ideally right after #18-#20, so the new repository behaviour is
   covered before it regresses.
4. **#16 + #17** — the auth story, once data is actually loading.
5. **#22, #23** — polish.

---

## 8. Open Questions for the Owner

1. **Is the MVC app still in scope, or is it being sunset?** It now works against the real API, so keeping it is viable; but the Angular client is the primary UI and the two will drift unless one is retired. Note the Angular client is currently the *less* correct of the two (G28, G29).
2. **What is the desired end‑state for the watch list?** The brief is explicit; the codebase has nothing (G17).
3. **Should the API's route be renamed to plural?** `/api/v1/Movie` is correct today but reads oddly next to `/api/v1/filmGenres`. Either rename the controller or add an alias route so clients can use `/api/v1/movies`.
4. **Should `ApplicationUser` carry extra fields** (e.g. `DisplayName`) for a richer profile UI?
5. **Is there a target deployment surface** (AKS, ACA, App Service, on‑prem)? This determines whether distributed rate limiting and a Postgres migration are in‑scope now.
6. **Is MongoDB ever returning?** The original brief mentioned MongoDB; the implementation uses EF Core + SQLite. Confirm that decision is final.

---

## 9. Quick Health Check Commands

Run from the repository root:

```bash
# Angular (needs Node 24.x)
cd apps/angular-client && npm ci && npm test && npm run build

# MVC
cd ../../apps/mvc-web \
  && dotnet restore MoviesWeb.sln \
  && dotnet build MoviesWeb.sln -c Release --no-restore \
  && dotnet test MoviesWeb.sln -c Release --no-build

# Web API
cd ../web-api \
  && dotnet restore MoviesAPI.sln \
  && dotnet build MoviesAPI.sln -c Release --no-restore \
  && dotnet test MoviesAPI.sln -c Release --no-build

# Container smoke test
docker compose up --build
```

Expected healthy result: all three test runs return `0` (345 tests total), all
three images build, and `curl localhost:8081/health` plus
`curl localhost:8080/health` return `{"status":"ok"}`.

### Verifying the API contract by hand

The API contract in this document was verified by running the API. To repeat
it:

```bash
# 1. Start the API with a real secret and a throwaway database
cd apps/web-api/MoviesApi
export ConnectionStrings__DefaultConnection="Data Source=/tmp/probe.db"
export EnableHttpsRedirection=false
export JwtSettings__SecretKey="$(openssl rand -base64 48)"
dotnet run

# 2. Register and sign in
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"email":"you@example.com","password":"Probe123!","confirmPassword":"Probe123!"}'

# 3. Note the route: /api/v1/Movie (singular). /api/v1/movies is a 404.
curl http://localhost:5000/api/v1/Movie -H "Authorization: Bearer <token>"
```

## 10. Doc Maintenance

This document should be re-reviewed at the end of every sprint. It was
last refreshed on **2026-10-29**, when the Angular client was pointed at the
live API contract, the MVC app was put on the UI mockup, and every app was
put behind a 70% line-coverage gate.

### Doc inventory

| Document | Purpose | Last verified |
| --- | --- | --- |
| `docs/PROJECT-STATUS.md` | This document — current state, gaps, risks | 2026-10-29 |
| `docs/ARCHITECTURE.md` | System shape, the real API contract, Mermaid diagram | 2026-10-25 |
| `docs/SECURITY-OVERVIEW.md` | OWASP posture per app | 2026-10-25 |
| `docs/SECRETS-MANAGEMENT.md` | Secret handling policy | 2026-09-14 |
| `docs/CI-CD.md` | Workflows, change detection, required checks | 2026-10-25 |
| `docs/CONTRIBUTING-RUNBOOK.md` | Contributor workflow | 2026-09-05 |
| `docs/DOTNET10-*` | The .NET 10 migration record | 2026-09-05 (historical) |
| `docs/PROJECT-SEPARATION-PLAN.md` | Proposal to split the monorepo | 2026-09-05 (proposal, not implemented) |

### Known doc drift

- `docs/PROJECT-SEPARATION-PLAN.md` is a historical record. It describes routes and states that have since moved
  on; read it for context, not for current behaviour.
- `apps/angular-client/README.md` is still the default Angular CLI scaffold
  text and does not describe the application.

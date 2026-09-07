# MoviesApp — Project Status

**Last reviewed:** 2026-09-05
**Scope:** Whole monorepo (`apps/angular-client`, `apps/mvc-web`, `apps/web-api`)
**Audience:** Senior developers, reviewers, maintainers
**Format:** Snapshot of current state, gaps, risks, and recommendations.

---

## 1. Executive Summary

MoviesApp is a three‑app monorepo that stores a personal movie collection. The Angular client and the MVC web app both consume a single OWASP‑hardened Web API. The repository layout, CI/CD pipeline, Dockerfiles, and documentation are mature; the **Web API has reached a production‑ready security posture (v3.0.0)**, while the **Angular client and MVC web app are still mid‑implementation** with significant functional gaps relative to the original brief in `MoviesApp Project.md`.

| Area | Status | Confidence |
| --- | --- | --- |
| Repository scaffolding, monorepo hygiene, CI/CD | ✅ Solid | High |
| Web API (`apps/web-api`) | ✅ Production‑grade security baseline | High |
| Angular client (`apps/angular-client`) | ⚠️ Skeleton + NgRx skeleton only | High |
| MVC web app (`apps/mvc-web`) | ⚠️ Stale shape, out of sync with API contract | Medium |
| Container / local orchestration | ✅ Working | High |
| Tests | ⚠️ Web API + Angular slices covered; MVC almost empty | High |
| Documentation | ✅ Extensive, slightly duplicative | High |

---

## 2. Repository Topology

```
MoviesApp/
├── apps/
│   ├── angular-client/    Angular 21 client (standalone + SSR + NgRx)
│   ├── mvc-web/           ASP.NET Core MVC (server-rendered, Razor)
│   └── web-api/           ASP.NET Core Web API (Clean Architecture: Domain → Data → Api)
├── docs/                  Architecture, CI/CD, security, migration plans
├── .github/               Agent guidance + filters + workflows
└── docker-compose.yml     Local full-stack
```

### Per‑app directories of note

- `apps/angular-client/src/app/store/movies/` — NgRx feature slice (actions, reducer, selectors, effects, tests).
- `apps/angular-client/src/services/` — `MovieService` (in‑memory seed data) and spec.
- `apps/angular-client/src/models/` — `Movie`, `MediaType`, `MovieFormat`.
- `apps/angular-client/mockoon/movie-api.json` — Mockoon environment; `scripts/mock-server.js` provides a Node fallback used by CI.
- `apps/web-api/MoviesApiDomain/Models/` — POCO entities: `Movie` (GUID), `MediaType`, `FilmGenre`, junction tables, `ApplicationUser`, `RefreshToken`, `AuditLog`.
- `apps/web-api/MoviesApiData/` — `MoviesDbContext` + EF Core migrations + repositories.
- `apps/web-api/MoviesApi/` — Controllers, DTOs, services (`AuthService`, `AuditLogService`), middleware, `Program.cs`.
- `apps/web-api/MoviesApi.Tests/` — xUnit tests for `MovieController`.
- `apps/mvc-web/MoviesWeb/` — Razor Views, `MoviesController`, `MoviesService` (HttpClient), `Movie`, `MovieQuery`, `MovieListModel`.

---

## 3. Current Implementation Snapshot

### 3.1 Angular client (`apps/angular-client`)

| Item | State |
| --- | --- |
| Angular version | 21.x with `@angular/ssr` 21.x |
| Styling | SCSS, Angular Material 21.x installed but no theme wired |
| Forms | `@angular/forms` available, not yet used |
| State | NgRx 21 (`store`, `effects`, `entity`, `router-store`, `store-devtools`) wired in `app.config.ts` |
| Routing | `movies` lazy route → `MoviesListComponent`; SSR uses `RenderMode.Prerender` for all routes |
| Data layer | `MovieService` is **in‑memory only** (3 seeded movies, 200 ms simulated latency). No HTTP. |
| Mock backend | Mockoon env + Node fallback `scripts/mock-server.js` exposing `/movies` (GET/POST) and `/movies/{id}` (GET) — CI spins it up for tests |
| SSR | Express server (`server.ts`) + AngularNodeAppEngine; health endpoint at `/health`; serves `/browser` static assets |
| Docker | Multi‑stage Node 24‑slim, listens on `:4000`, `/health` healthcheck |
| Tests | `movie-service.spec.ts` ✅, `movies.reducer.spec.ts` ✅, `movies.effects.spec.ts` ✅, `movies.selectors.spec.ts` ✅, `app.spec.ts` ✅ |
| Components | Only `MoviesListComponent` exists — no header, no watch list, no search bar, no add/edit form |

> **Verdict:** The plumbing (NgRx, SSR, Mockoon, Docker, tests) is in place. There is exactly **one route** and **one feature component**. The brief calls for several components and pages that are not implemented yet.

### 3.2 Web API (`apps/web-api`)

| Layer | Notes |
| --- | --- |
| Target framework | `net10.0` (migrated in v2.0.0) |
| Clean architecture | Domain (POCOs) → Data (DbContext + Repositories + Migrations) → Api (Controllers, DTOs, Services, Middleware) |
| Persistence | EF Core 10 + SQLite (`EnsureCreated` on startup). Seed data for `MediaType` and `FilmGenre` |
| Auth | ASP.NET Core Identity + JWT (HS256) + refresh tokens (`RefreshToken` table) |
| Authorization | `[Authorize]` policies: `UserOnly` (default), `AdminOnly` |
| Object‑level ownership | `Movie.OwnerId` → every movie endpoint filters by `User.FindFirst(NameIdentifier)` |
| Cross‑cutting middleware | `SecurityHeadersMiddleware`, `ExceptionHandlingMiddleware`, `RateLimitingMiddleware`, `AuditLoggingMiddleware` |
| Rate limiting | 100 req/min (read), 10 req/min (write), keyed by user ID or IP; in‑memory `ConcurrentDictionary` |
| Audit logging | `AuditLogService` writes CREATE/UPDATE/DELETE rows; controllers + middleware both emit entries |
| Security headers | CSP, HSTS (prod), X‑Frame‑Options DENY, X‑Content‑Type‑Options nosniff, Permissions‑Policy |
| API versioning | Route prefix `/api/v1/[controller]`; Swagger enabled in dev only |
| Tests | `MovieControllerTests` covers GET/POST/PUT/DELETE happy paths + 404 + null handling |
| Docker | SDK 10.0 → ASP.NET 10.0, port 8080, `/health` healthcheck |

**Endpoints (verified in code):**

- `POST /api/v1/auth/register` — anonymous
- `POST /api/v1/auth/login` — anonymous
- `POST /api/v1/auth/refresh` — anonymous
- `POST /api/v1/auth/logout` — authorize
- `GET /api/v1/auth/me` — authorize
- `GET /api/v1/movies` — `UserOnly`, paginated, scoped to caller
- `GET /api/v1/movies/{id:guid}` — `UserOnly`, ownership‑checked
- `POST /api/v1/movies` — `UserOnly`, audit logged
- `PUT /api/v1/movies/{id:guid}` — `UserOnly`, audit logged
- `DELETE /api/v1/movies/{id}` — `UserOnly`, audit logged (note: `{id:guid}` constraint not enforced on DELETE in current code)
- `GET /api/v1/mediatypes` + `GET /api/v1/mediatypes/{id}` — anonymous
- `GET /api/v1/filmgenres` + `GET /api/v1/filmgenres/{id}` — anonymous
- `GET /health` — anonymous, used by Docker healthcheck

> **Verdict:** This is the strongest part of the repository. The OWASP Top 10 baseline (8/10) is real, not aspirational — it is implemented in code, migrations, and tests.

### 3.3 MVC web app (`apps/mvc-web`)

| Item | State |
| --- | --- |
| Target framework | `net10.0` |
| Razor views | `Views/` directory present (Home + Error + Privacy). Full Razor surface not enumerated here, but **no `Index.cshtml` for `MoviesController`** is wired in the routes that were reviewed. |
| Controllers | `HomeController.cs`, `MoviesController.cs` |
| Services | `MoviesService` (HttpClient‑backed) wired via `AddHttpClient<IMoviesService, MoviesService>` against `MoviesApi:BaseUrl` |
| Models | `Movie` (int Id, Title, Description, Genre, Director, ReleaseYear, ReleaseDate, CriterionTitle nav), `MovieQuery` (allow‑listed keys: `title`, `genre`, `director`, `releaseYear`), `CriterionTitle`, `User`, `UserFavouriteMovies` |
| ViewModels | `MovieListModel` with paging metadata |
| Tests | `MoviesWeb.Tests` project exists (xUnit). Coverage not surveyed in detail, but `MoviesController` logic is thin. |
| Docker | SDK 10.0 → ASP.NET 10.0, port 8080, `/health` healthcheck |
| Compose wiring | `mvc-web` depends on `web-api` (healthy); calls API via `MoviesApi__BaseUrl` env |

> **Verdict:** The MVC app is **mis‑aligned with the API contract**. Its `Movie` model uses `int Id` and a `Director` field that the API does not expose; the service posts GETs that the API does not implement (e.g. `movies/query`, `movies/{id}` against the v1 path). It is essentially a placeholder.

---

## 4. Architecture Findings

### 4.1 Strengths

- **Monorepo with per‑app CI gating** — change detection in `.github/filters.yaml` scopes workflows per app, so docs edits do not waste Docker minutes.
- **Web API clean architecture** — Domain has zero dependencies; Data depends on Domain; Api composes both. This is rare to see actually enforced.
- **OWASP implementation is genuine** — JWT with refresh‑token rotation, GUID resource IDs, ownership filtering at the repository, per‑user rate limiting, audit logging of CREATE/UPDATE/DELETE, secure headers middleware, generic error responses, Swagger disabled in production.
- **Angular NgRx setup is idiomatic** — `createActionGroup`, `EntityAdapter`, `createEffect`, mock‑actions in tests. The store is wired with `provideRouterStore` so route state is replayable.
- **SSR is correct** — `provideServerRendering(withRoutes(serverRoutes))` and a working Express handler with health endpoint.
- **Docker is clean** — multi‑stage builds with explicit runtime images, non‑root‑friendly (no root user, but worth pinning explicitly later), healthchecks on every service.

### 4.2 Gaps and Inconsistencies

| # | Gap | Where | Impact |
| --- | --- | --- | --- |
| G1 | Only one feature route exists; no header, no watch list, no add/edit dialog, no search bar, no paging UI | `apps/angular-client/src/app/features/`, `app.routes.ts` | Brief is mostly unimplemented |
| G2 | `MovieService` returns hard‑coded seed data; no HTTP layer | `apps/angular-client/src/services/movie-service.ts` | Front end cannot talk to the real API |
| G3 | No authentication wiring in Angular (no interceptor, no guards, no login UI) | `app.config.ts` | The “secure” Web API cannot be exercised from the front end |
| G4 | `movies-list.component.ts` uses NgRx but mixes imperative subscribe patterns via `async` pipe on `loading$`/`error$` (acceptable) yet dispatches `removeMovie({ id })` where `id` is `number` while the API uses `Guid` | feature component + `movies.actions.ts` | Will throw at runtime once HTTP is wired; type contract drift between front and back end |
| G5 | MVC `Movie` model (`int Id`, `Director`) does not match the API (`Guid Id`, no `Director`, separate `MovieMediaType` and `MovieFilmGenre` join entities) | `apps/mvc-web/MoviesWeb/Models/Movie.cs` vs `apps/web-api/MoviesApiDomain/Models/Movie.cs` | MVC app cannot consume the real API without a contract rewrite |
| G6 | `MoviesService` calls `movies/query` and `movies/{id}` which the v1 API does not expose (v1 has `movies/{id:guid}` GET and no query endpoint) | `apps/mvc-web/MoviesWeb/Services/MoviesService.cs` | MVC app would 404 against the v1 API |
| G7 | Rate limiting is in‑memory (`ConcurrentDictionary`) — does not survive restart or scale across instances | `apps/web-api/MoviesApi/Middleware/RateLimitingMiddleware.cs` | OK for single instance; needs Redis/distributed cache for HA |
| G8 | `RateLimitingMiddleware` is registered via `app.UseMiddleware<>()` but its `RateLimitOptions` is not actually registered in DI (configured via `Configure<>` without a corresponding `services.Add…`). The middleware would resolve to a default‑constructed `RateLimitOptions` | `Program.cs` lines around `Configure<RateLimitOptions>` | **Bug** — rate limiting is using default values, not the configured 100/10 |
| G9 | `AuthService.GenerateJwtToken()` calls `_userManager.GetRolesAsync(user).Result` — `.Result` on an async call inside a non‑async method blocks the thread | `apps/web-api/MoviesApi/Services/AuthService.cs` | Performance hazard under load |
| G10 | `AuthService.RefreshTokenAsync` swallows all exceptions and returns `null` — no logging | `apps/web-api/MoviesApi/Services/AuthService.cs` | Silent failures in auth flow |
| G11 | `MovieController.Delete` route is `{id}` without `:guid` constraint, but `GetById` and `Update` use `{id:guid}`. Inconsistent routing | `apps/web-api/MoviesApi/Controllers/MovieController.cs` | Minor: any string matches DELETE |
| G12 | `AuditLoggingMiddleware` reads the request body by enabling buffering, but the controller also constructs an audit log with the same body — duplicate writes for the same operation | `AuditLoggingMiddleware.cs` vs controllers | Audit log duplication per mutation |
| G13 | MVC `MoviesController.Index` returns `NotFound` if `movies` is `null`; `MoviesService.GetMoviesAsync` returns `Array.Empty<Movie>()` on null, so this branch is effectively dead. But the controller still creates a view with an empty model | `apps/mvc-web/.../MoviesController.cs` | Misleading |
| G14 | `MoviesService.UpdateMovieAsync` calls `EnsureSuccessStatusCode` after already returning on non‑success — dead code | `apps/mvc-web/.../MoviesService.cs` | Cleanup |
| G15 | No refresh‑token use from Angular; no `AuthInterceptor`, no token storage strategy documented in code | `apps/angular-client/src/app/app.config.ts` | Half of the security value of the API is invisible to clients |
| G21 | Two known‑vulnerability packages (`AutoMapper` 14.0.0, `SQLitePCLRaw.lib.e_sqlite3` 2.1.11) flagged by the September Search Sprint as pre‑flight work for the Kubernetes push. Bumping them is part of [Phase 0 of the deployment plan](k8s/PHASE-0-CHECKLIST.md), and the README's Kubernetes section should call out the bumps | `apps/web-api/MoviesApi/MoviesApi.csproj`, `apps/web-api/MoviesApiData/MoviesApiData.csproj` | Security hygiene; visible to anyone running `dotnet list package --vulnerable` |
| G16 | No query/filter endpoint on the API, yet the front‑end brief and MVC `MovieQuery` both describe rich filtering (title, genre, criterion, criterion number, year, favourite) | Web API controllers | Brief is unimplemented on the server |
| G17 | Watch List is in the brief but has no model, service, controller, or endpoint anywhere | All apps | Brief is unimplemented |
| G18 | No health checks for Angular SSR (Dockerfile `HEALTHCHECK` uses `node -e "fetch(...)"` which is fine, but a graceful startup probe would help) | `apps/angular-client/Dockerfile` | Minor |
| G19 | No favicon / `manifest.webmanifest` wired; SSR + hydration work but PWA artefacts are absent | `apps/angular-client/src/index.html` | Minor |
| G20 | The Mockoon server used in CI only supports GET/POST `/movies` and GET `/movies/{id}`. The Angular test suite does not appear to exercise HTTP — it injects a stubbed `MovieService` instead, which is fine, but the documented intent of Mockoon as the contract source is weakened | `scripts/mock-server.js` | Doc drift |

### 4.3 Security Notes

- **Strengths:** Identity + JWT + refresh tokens, object‑level ownership, secure headers, rate limiting (config), audit logs, GUID IDs, CORS locked to a known origin list (`http://localhost:4200`, `https://your-production-domain.com`).
- **To address:**
  - CORS in `Program.cs` is hard‑coded to placeholder origins. It should be sourced from configuration so prod domains are not edited in source.
  - Rate limit options DI bug (G8) — the configured limits are not the limits actually applied.
  - `RequireNonAlphanumeric = true` for passwords is opinionated and reduces accessibility; consider making it configurable.
  - Refresh‑token revocation is not currently enforced with rotation detection (i.e. reuse of a revoked token does not trigger cascade revoke — OWASP recommends this).
  - No CSRF surface on the API (Bearer token only) — fine.
  - Audit middleware swallows JSON parse errors silently — acceptable, but consider capturing into a separate failure log.

---

## 5. Testing Coverage Snapshot

| App | Tests present | Notes |
| --- | --- | --- |
| Angular | `movie-service.spec.ts`, `movies.reducer.spec.ts`, `movies.effects.spec.ts`, `movies.selectors.spec.ts`, `app.spec.ts` | Solid for what exists. No component integration tests, no e2e. CI runs `npm test` (Vitest) + mock server. |
| Web API | `MoviesApi.Tests/MovieControllerTests.cs` (xUnit + Moq) | Covers happy paths and 404 for movie CRUD. No tests for `AuthService`, `AuditLogService`, middleware, repository. |
| MVC | `MoviesWeb.Tests/` project exists with empty body per survey | No meaningful coverage of controller / service. |

> **Recommendation:** Raise the bar in two places: (1) Web API middleware (`RateLimiting`, `ExceptionHandling`, `SecurityHeaders`, `AuditLogging`) and `AuthService` need tests; (2) MVC needs at least one controller smoke test.

---

## 6. CI/CD Status

| Workflow | Trigger | Behaviour |
| --- | --- | --- |
| `validate-apps.yml` | PR, schedule, manual | Runs the changed app’s restore/build/test, plus nightly full validation. Concurrency group `validate-apps-${{ github.ref }}`. |
| `container-images.yml` | Manual + scheduled full | Per‑app Docker build (no push). |
| `release-images.yml` | Manual + `release-*` tag push | Per‑app publish to GHCR with deterministic tag naming. |

Path‑based scoping lives in `.github/filters.yaml` and is shared across workflows. Required status checks recommended in `docs/CI-CD.md`: `angular-client`, `mvc-web`, `web-api`, `build-angular-client`, `build-mvc-web`, `build-web-api`.

**Suggested follow‑ups:**

- Pin `actions/*` and `dorny/paths-filter` versions explicitly (they are pinned to major tags today; consider SHA pinning for high‑value workflows).
- Add `concurrency` cancel‑in‑progress behaviour consistently (release workflow currently does not cancel).
- Add a CodeQL or Dependabot config for NuGet and npm (none observed in the repo).

---

## 7. Risks and Recommendations (Prioritised)

### P0 — correctness blockers

1. **Fix rate‑limit DI bug (G8).** Register `RateLimitOptions` so the configured 100/10 limits actually apply, otherwise the deployed API is silently running at ASP.NET defaults.
2. **Resolve front/back‑end type drift (G4, G6).** Decide on `Guid` vs `number` for `Id` in Angular `Movie`, and align MVC service to call real v1 paths with GUIDs.
3. **Rewrite the MVC contract (G5, G13, G14).** Either rebuild MVC against the real API or document it as a placeholder.

### P1 — feature completeness vs the brief

4. **Implement remaining Angular components:** `MovieHeaderComponent` (search bar + count + title), `MovieWatchListComponent`, query bar, paging, add/edit form.
5. **Implement watch list end‑to‑end (G17).** Domain model, API endpoints, NgRx slice, components, tests.
6. **Implement query endpoint on the API (G16).** Replace `MovieQuery` in MVC and the brief’s criteria with a real `/movies/search` (or query‑string) endpoint.

### P1 — security hardening

7. **CORS from configuration (Security Notes).** Move the origin list out of `Program.cs`.
8. **Distributed rate limiting** for multi‑instance deployments (Redis‑backed `RateLimiter`).
9. **Refresh‑token reuse detection (Security Notes).** On a revoked token being presented, cascade‑revoke all descendants and force re‑login.
10. **Avoid `.Result` on async (`G9).`** Convert `GenerateJwtToken` to `async`.
11. **Audit‑log de‑duplication (`G12`).** Pick one source of truth (controller) or the middleware, not both.

### P2 — hygiene

12. **Add tests for Web API middleware, `AuthService`, `AuditLogService`, and `MovieRepository`** (especially the ownership‑check paths).
13. **Add an integration test that boots the API in‑memory** (e.g. `WebApplicationFactory`) and exercises auth → CRUD with ownership.
14. **Add Angular component tests** for `MoviesListComponent` and any new components (use Vitest + Angular TestBed).
15. **Add at least one smoke test for `MoviesController`** in MVC.
16. **Enable Dependabot** for both npm and NuGet ecosystems.
17. **Pin GitHub Actions by SHA** for high‑value workflows.
18. **Document the env vars** required by each Docker image (`JwtSettings__*`, `MoviesApi__BaseUrl`, CORS origins, etc.) in `docs/`.
19. **Move `apps/angular-client/src/services/` under `src/app/services/`** to match the convention documented in `AGENTS.md` and `copilot-instructions.md`. Today the folder is outside `app/`, breaking the documented pattern.
20. **Drop the dead HTTP comments** in `apps/web-api/MoviesApi/Program.cs` (the commented `app.MapGet("/movies")` block at the bottom of `Program.cs`).

---

## 8. Open Questions for the Owner

1. Is the MVC app still in scope, or is it being sunset in favour of the Angular client only?
2. What is the desired end‑state for the watch list? The brief is explicit; the codebase has nothing.
3. Should `ApplicationUser` carry extra fields (e.g. `DisplayName`) for a richer profile UI?
4. Is there a target deployment surface (AKS, ACA, App Service, on‑prem)? This determines whether distributed rate limiting and Postgres migration are in‑scope now.
5. Is MongoDB ever returning? The original brief mentioned MongoDB; the implementation uses EF Core + SQLite. Confirm that decision is final.

---

## 9. Quick Health Check Commands

Run from the repo root:

```bash
# Angular
cd apps/angular-client && npm ci && CI=true npm test && npm run build

# MVC
cd ../../apps/mvc-web && dotnet restore MoviesWeb.sln && dotnet build MoviesWeb.sln -c Release --no-restore && dotnet test MoviesWeb.sln -c Release --no-build

# Web API
cd ../web-api && dotnet restore MoviesAPI.sln && dotnet build MoviesAPI.sln -c Release --no-restore && dotnet test MoviesAPI.sln -c Release --no-build

# Container smoke test
docker compose up --build
```

Expected healthy result: all three test runs return `0`, all three images build, and `curl localhost:8081/health` plus `curl localhost:8080/health` return `{"status":"ok"}`.

---

## 10. Suggested Doc Updates

This document (`docs/PROJECT-STATUS.md`) should be re‑reviewed at the end of every sprint. Track changes via the existing `CHANGELOG.md` and link any P0 fixes to a release tag.

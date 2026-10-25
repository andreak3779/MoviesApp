# CI/CD Overview

The monorepo now uses root-owned GitHub Actions workflows under `.github/workflows/`.

## Workflows

| Workflow | Trigger | What it does |
| --- | --- | --- |
| `validate-apps.yml` | PR, schedule, manual (`workflow_dispatch` with a target app) | Per-app restore, build and test. |
| `container-images.yml` | Manual + scheduled full | Per-app Docker image build (no push). |
| `release-images.yml` | Manual + `release-*` tag push | Per-app publish to GHCR (and ACR). |
| `security.yml` | Every push | Repository-wide secret scanning. |

### What `validate-apps.yml` runs per app

- **`angular-client`** — `npm ci`, starts the mock API
  (`scripts/mock-server.js`) and waits for `/health`, then `npm test` and
  `npm run build`.
- **`mvc-web`** — `dotnet restore`, `dotnet build -c Release`, then
  `dotnet test -c Release --no-build`.
- **`web-api`** — `dotnet restore`, `dotnet build -c Release`, then
  `dotnet test -c Release --no-build`.

All three .NET jobs use `dotnet-version: 10.0.x` with a NuGet cache keyed on
the project's `.csproj`/`.sln` files. The Angular job uses Node `24.11.1` with
an npm cache keyed on `package-lock.json`.

Concurrency is `validate-apps-${{ github.ref }}` with `cancel-in-progress: true`,
so a new push to a branch cancels the older run.

## Change Detection

Path filters are centralized in `.github/filters.yaml` and are shared by every
workflow.

| Filter | Matches | Effect |
| --- | --- | --- |
| `angular` | `apps/angular-client/**` | Angular job only |
| `mvc` | `apps/mvc-web/**` | MVC job only |
| `webapi` | `apps/web-api/**` | Web API job only |
| `shared` | `.github/filters.yaml`, `.github/workflows/**`, `README.md`, `AGENTS.md`, `.env.example`, `docker-compose.yml`, `docs/**`, `.pre-commit-config.yaml`, `.gitleaks.toml`, `.gitignore` | **All** validation jobs |
| `security` | `**` (ignoring `docs/**`) | The security workflow always runs |

Container builds are limited to app and container-related changes so
docs-only updates do not spend Docker minutes.

## Expected Required Checks

Suggested branch-protection checks:

- `angular-client`
- `mvc-web`
- `web-api`
- `build-angular-client`
- `build-mvc-web`
- `build-web-api`

## Local Validation Commands

Run the same commands locally before opening a pull request:

```bash
cd apps/angular-client && npm ci && CI=true npm test && npm run build
cd apps/mvc-web && dotnet restore MoviesWeb.sln && dotnet build MoviesWeb.sln --configuration Release --no-restore && dotnet test MoviesWeb.sln --configuration Release --no-build
cd apps/web-api && dotnet restore MoviesAPI.sln && dotnet build MoviesAPI.sln --configuration Release --no-restore && dotnet test MoviesAPI.sln --configuration Release --no-build
```

Expected: **26** Angular tests, **32** MVC tests, **35** Web API tests — 93
passing, 0 failing.

### A known build warning

`SQLitePCLRaw.lib.e_sqlite3` 2.1.11 carries a high-severity advisory
([GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q)) and
is pulled in transitively by `Microsoft.EntityFrameworkCore.Sqlite` 10.0.0. The
`NU1903` warning appears on every Web API build and does **not** fail the
build. See [G21](PROJECT-STATUS.md).

## Release Images

Use the `Release Images` workflow to publish one app image or all app images to GHCR.

- Manual release: run the workflow and choose the target app.
- Tag release: push a tag matching `release-*`.
- Published image names:
  - `ghcr.io/<owner>/moviesapp-angular-client`
  - `ghcr.io/<owner>/moviesapp-mvc-web`
  - `ghcr.io/<owner>/moviesapp-web-api`

Deployment automation remains intentionally separate from image publishing so environment-specific secrets and approvals can be added later with GitHub Environments.

## Follow-ups

- Pin `actions/*` and `dorny/paths-filter` by SHA for high-value workflows.
- `release-images.yml` does not use `cancel-in-progress`; the other three do.
- No Dependabot configuration for npm or NuGet.
- No CodeQL workflow — `security.yml` covers secret scanning only.

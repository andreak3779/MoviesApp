# MoviesApp Monorepo

MoviesApp is a secure, production-ready movie collection management system organized as a single monorepo containing three applications:

- Angular 22 client (with optional SSR)
- ASP.NET Core MVC web app
- ASP.NET Core Web API (partial OWASP Top 10 coverage — see [SECURITY-OVERVIEW.md](docs/SECURITY-OVERVIEW.md))

This layout allows independent app development while keeping CI/CD, container orchestration, and contribution workflows centralized.

> **Current state.** All three apps build and all three test suites are green
> (**345 tests**: Angular 92, Web API 144, MVC 109). Every app is pointed at
> the live API contract, and each one is held to a **70% line-coverage gate**
> in CI (Angular 79.07%, MVC 93.42%). The Angular client and the MVC app
> both render the MoviesApp UI Mockup. See
> [PROJECT-STATUS.md](docs/PROJECT-STATUS.md) for the full picture.

## Repository Layout

- `apps/angular-client`: Angular 22 client (with optional SSR)
- `apps/mvc-web`: ASP.NET Core MVC app
- `apps/web-api`: ASP.NET Core Web API (partial OWASP Top 10 coverage — see
  [docs/SECURITY-OVERVIEW.md](docs/SECURITY-OVERVIEW.md))
- `.github/workflows`: Root-owned CI/CD workflows
- `docs`: Architecture, CI/CD, contributor, security, and deployment notes
- `docker-compose.yml`: Local full-stack container orchestration

## Prerequisites

- Node.js 24.x (`apps/angular-client/package.json` declares `node: ^24.0.0`)
- npm
- .NET SDK 10.x (.NET 10.0.107 or later) ✅ **Updated to .NET 10**
- Docker and Docker Compose (optional, for containerized local runs)

## Quickstart

### 1. Clone and enter the repository

```bash
git clone <repo-url>
cd MoviesApp
```

### 2. Angular client

```bash
cd apps/angular-client
npm ci
npm start
```

Useful commands:

```bash
npm test
npm run build
npm run serve:ssr:movies-app
```

### 3. MVC app

```bash
cd apps/mvc-web
dotnet restore MoviesWeb.sln
dotnet run --project MoviesWeb/MoviesWeb.csproj
```

Useful commands:

```bash
dotnet build MoviesWeb.sln --configuration Release
dotnet test MoviesWeb.sln --configuration Release
```

### 4. Web API app (partial OWASP Top 10 coverage)

```bash
cd apps/web-api/MoviesApi

# Initialize secrets (required)
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 32)"

# Restore and run
dotnet restore
dotnet run
```

Useful commands:

```bash
dotnet build MoviesAPI.sln --configuration Release
dotnet test MoviesAPI.sln --configuration Release
```

**Security Features:**
- JWT authentication with refresh tokens
- Role-based authorization
- Rate limiting (100 req/min read, 10 req/min write)
- Request pagination
- Comprehensive audit logging
- Security headers (CSP, HSTS, etc.)
- Input validation
- GUID-based resource IDs

See `apps/web-api/README.md` for complete API documentation.

## Run the Full Stack with Docker

From the repository root:

```bash
docker compose up --build
```

Default exposed ports:

- Angular client: 4000
- MVC app: 8080
- Web API: 8081

You can override ports and API routing with environment variables in `.env`.
Use `.env.example` as the baseline.

## CI/CD

Root-level workflows are the source of truth for automation:

- `Validate Apps`: per-app validation with path-based change detection
- `Container Images`: per-app Docker image build validation
- `Release Images`: manual and tag-based image publishing to GHCR

See `docs/CI-CD.md` for details, required checks, and release image naming.

## Kubernetes

Not implemented yet. The three apps are containerized and run together under Docker Compose
(see above); Kubernetes manifests and an Azure deployment are planned future work.

---

## Tracked Work

The open defects found while verifying the API contract are tracked as
**10 GitHub issues** under the milestone
[`4.2.0 - API contract and client correctness`](https://github.com/andreak3779/MoviesApp/milestone/1):

| Area | Issues |
| --- | --- |
| Angular — make the UI load live data | [#14](https://github.com/andreak3779/MoviesApp/issues/14) route, [#15](https://github.com/andreak3779/MoviesApp/issues/15) casing, [#16](https://github.com/andreak3779/MoviesApp/issues/16) token refresh, [#17](https://github.com/andreak3779/MoviesApp/issues/17) 401 handling |
| Web API — fix the repository defects | [#18](https://github.com/andreak3779/MoviesApp/issues/18) POST 500, [#19](https://github.com/andreak3779/MoviesApp/issues/19) PUT data loss, [#20](https://github.com/andreak3779/MoviesApp/issues/20) GET formats |
| Tests and hardening | [#21](https://github.com/andreak3779/MoviesApp/issues/21) integration tests, [#22](https://github.com/andreak3779/MoviesApp/issues/22) paginator, [#23](https://github.com/andreak3779/MoviesApp/issues/23) audit logging |

See [PROJECT-STATUS.md](docs/PROJECT-STATUS.md) for the full analysis behind each one.

## Contributor Workflow

Use the monorepo runbook for branch strategy, subtree imports, and gate checks:

- `docs/CONTRIBUTING-RUNBOOK.md`

## Related Documentation

| Document | What it covers |
| --- | --- |
| [`docs/PROJECT-STATUS.md`](docs/PROJECT-STATUS.md) | **Start here.** Verified current state of all three apps, the open gaps (G1-G36), and prioritised recommendations. |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | System shape, the verified API contract, middleware order, and the component diagram. |
| [`docs/moviesapp-diagram.mmd`](docs/moviesapp-diagram.mmd) | Mermaid source for the architecture diagram (26 nodes, 33 edges). |
| [`docs/CI-CD.md`](docs/CI-CD.md) | All four GitHub Actions workflows, change-detection filters, and expected results. |
| [`docs/SECURITY-OVERVIEW.md`](docs/SECURITY-OVERVIEW.md) | What the API actually enforces, and where it falls short. |
| [`docs/SECRETS-MANAGEMENT.md`](docs/SECRETS-MANAGEMENT.md) | Where secrets live, how they are supplied, and how leaks are handled. |
| [`docs/CONTRIBUTING-RUNBOOK.md`](docs/CONTRIBUTING-RUNBOOK.md) | Branch strategy, gate checks, and expected test results. |
| [`AGENTS.md`](AGENTS.md) | Conventions for AI coding agents working in this repository. |

Per-app documentation lives with the app: [`apps/angular-client/README.md`](apps/angular-client/README.md),
[`apps/mvc-web/README.md`](apps/mvc-web/README.md),
[`apps/web-api/README.md`](apps/web-api/README.md).

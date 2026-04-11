# MoviesApp Monorepo

MoviesApp is now organized as a single monorepo containing three applications:

- Angular client
- ASP.NET Core MVC web app
- ASP.NET Core Web API

This layout allows independent app development while keeping CI/CD, container orchestration, and contribution workflows centralized.

## Repository Layout

- `apps/angular-client`: Angular 21 client (with optional SSR)
- `apps/mvc-web`: ASP.NET Core MVC app
- `apps/web-api`: ASP.NET Core Web API
- `.github/workflows`: Root-owned CI/CD workflows
- `docs`: Architecture, CI/CD, and contributor documentation
- `docker-compose.yml`: Local full-stack container orchestration

## Prerequisites

- Node.js 24.x (Angular app engines currently target 24.11.1)
- npm
- .NET SDK 10.x
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

### 4. Web API app

```bash
cd apps/web-api
dotnet restore MoviesAPI.sln
dotnet run --project MovieCollectionApi/MovieCollectionApi.csproj
```

Useful commands:

```bash
dotnet build MoviesAPI.sln --configuration Release
dotnet test MoviesAPI.sln --configuration Release
```

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

## Contributor Workflow

Use the monorepo runbook for branch strategy, subtree imports, and gate checks:

- `docs/CONTRIBUTING-RUNBOOK.md`

## Related Documentation

- `docs/ARCHITECTURE.md`
- `docs/CI-CD.md`
- `docs/moviesapp-diagram.mmd`
- `AGENTS.md`

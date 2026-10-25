# MoviesApp Monorepo

MoviesApp is a secure, production-ready movie collection management system organized as a single monorepo containing three applications:

- Angular 22 client (with optional SSR)
- ASP.NET Core MVC web app
- ASP.NET Core Web API (partial OWASP Top 10 coverage — see [SECURITY-OVERVIEW.md](docs/SECURITY-OVERVIEW.md))

This layout allows independent app development while keeping CI/CD, container orchestration, and contribution workflows centralized.

> **Current state.** All three apps build and all three test suites are green
> (**93 tests**: Angular 26, Web API 35, MVC 32). The MVC app and the Web API
> have been verified end-to-end against each other. The Angular client is a
> complete application but is **not yet pointed at the live API contract** —
> it calls `/api/v1/movies` (the real route is the singular `/api/v1/Movie`)
> and expects PascalCase while the API emits camelCase. See
> [PROJECT-STATUS.md](docs/PROJECT-STATUS.md) for the full picture, including
> three open API defects.

## Repository Layout

- `apps/angular-client`: Angular 22 client (with optional SSR)
- `apps/mvc-web`: ASP.NET Core MVC app
- `apps/web-api`: ASP.NET Core Web API (partial OWASP Top 10 coverage — see
  [docs/SECURITY-OVERVIEW.md](docs/SECURITY-OVERVIEW.md))
- `.github/workflows`: Root-owned CI/CD workflows
- `docs`: Architecture, CI/CD, contributor, security, and deployment documentation
- `docker-compose.yml`: Local full-stack container orchestration
- `k8s/`: AKS manifests (Deployment, Service, ConfigMap, Secret, PVC, Ingress)

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
- `Release Images`: manual and tag-based image publishing to GHCR (and ACR — see Kubernetes section)

See `docs/CI-CD.md` for details, required checks, and release image naming.

## Kubernetes

The application is deployable to **Azure Kubernetes Service (AKS)**. Images are published to **Azure Container Registry (ACR)** alongside GHCR. The full plan lives in [`docs/KUBERNETES-PLAN.md`](docs/KUBERNETES-PLAN.md); the working companion with manifest sketches and commands is in [`docs/k8s/IMPLEMENTATION-PLAN.md`](docs/k8s/IMPLEMENTATION-PLAN.md).

### Cluster topology

- **Cluster:** AKS Standard tier, 1 node pool, 1 node (`Standard_B2s`), kubelet managed identity bound to `AcrPull` on the ACR (no `imagePullSecret`).
- **Registry:** ACR (Standard SKU). Images tagged by short SHA. `release-images.yml` publishes to ACR in addition to GHCR.
- **Ingress:** AKS **Application Routing** addon (Azure‑managed NGINX). `web-api` stays on `ClusterIP` — it is not exposed publicly. CORS is not exercised in‑cluster because only `mvc-web` calls the API.
- **State:** SQLite on an Azure Disk CSI PVC (`ReadWriteOnce`, 5Gi). The API runs at **one replica** — see the constraint section below.

### Constraints

These are deliberate trade‑offs, not shortcuts.

- **SQLite forces a single API replica.** A file‑based database on a `ReadWriteOnce` volume cannot be shared by multiple writers. The API's `Deployment` has `replicas: 1` and a `PodDisruptionBudget` to prevent voluntary disruption during the backup window. The proper fix is Azure SQL or PostgreSQL with a connection string from a Secret, which would unlock horizontal scaling and remove the PVC entirely. That work is documented as follow‑up in the implementation plan.
- **The Angular client calls the API but is not wired to the live contract yet.** `MovieService` is an HTTP client, but it targets `/api/v1/movies` while the API's route is the singular `/api/v1/Movie`, and it expects PascalCase while the API emits camelCase. Until that is corrected the Angular screens load no data. `mvc-web` is the client that currently works against the live API. Fixing the Angular client is application work and is deliberately outside the deployment plan.
- **The Web API is internal.** Only `mvc-web` reaches it. CORS origins come from `Cors:AllowedOrigins` in configuration (they are no longer hard-coded), and the API refuses to start in Production if the list still contains a placeholder origin. If the API is ever exposed publicly, that list must be set for the real domain first.

### Configuration and secrets

- **ConfigMap** `moviesapp-config` carries `ASPNETCORE_URLS`, `MoviesApi__BaseUrl`, `EnableHttpsRedirection`, and CORS origins (when applicable).
- **Secret** `moviesapp-secrets` carries `JwtSettings__SecretKey`. The committed placeholder in `appsettings.json` is removed — the app fails loudly at startup if the key is absent. The dev workflow still uses `dotnet user-secrets`.
- **Backup:** a daily `CronJob` copies `movies.db` from the PVC to a blob storage account using the cluster's workload identity.

### Security posture

- All three containers run as non‑root with `readOnlyRootFilesystem: true`, `allowPrivilegeEscalation: false`, and `capabilities.drop: [ALL]`.
- The ACR pull is via managed identity; no long‑lived registry credentials exist in the cluster.
- Web API image carries the same security middleware stack as production (`SecurityHeadersMiddleware`, `ExceptionHandlingMiddleware`, `RateLimitingMiddleware`, JWT + refresh tokens, GUID resource IDs, audit logging).
- `AutoMapper` was raised to **16.2.0** (it was 14.0.0 when
  [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x) was
  open). **`SQLitePCLRaw.lib.e_sqlite3` is still on 2.1.11** and still carries
  the high‑severity advisory
  [GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q) —
  it is a transitive dependency of `Microsoft.EntityFrameworkCore.Sqlite`
  10.0.0 and is **not yet** bumped. The NU1903 warning appears on every build
  until it is. See [G21](docs/PROJECT-STATUS.md).

### Local validation

Before applying manifests to a real cluster:

```bash
# Confirm the source compose stack is still green (this is the Phase 0 gate)
docker compose up --build

# Render the manifests and confirm they parse
kubectl apply --dry-run=client -f k8s/
```

### Teardown

To stop the cluster and remove all cloud resources:

```bash
az group delete --name moviesapp-k8s --yes --no-wait
```

1This removes the AKS cluster, ACR, Container Apps environment, backup storage account, and Log Analytics workspace in one operation.

### Cost

The default sprint configuration is **AKS Free tier + `Standard_B1s` + Container Insights off + teardown between sessions**, which keeps the persistent ACR cost at roughly **~$5/month** and the cluster compute at **$0 when not running**. Recreate from [`docs/k8s/MANIFESTS-HOWTO.md`](docs/k8s/MANIFESTS-HOWTO.md) in ~10 minutes when needed for a demo.

| Configuration | Monthly if left running | Monthly if torn down between sessions |
| --- | --- | --- |
| AKS **Free** + `Standard_B1s` (default) | ~$8–12 | ~$5 |
| AKS **Free** + `Standard_B2s` + Container Insights | ~$35–50 | ~$5 |
| AKS **Standard** + `Standard_B2s` + Container Insights | ~$115–125 | ~$5 |

The AKS control plane dominates at Standard tier ($73/month). Use Standard only if a specific job application asks for an SLA‑backed cluster.

> **Status of this block.** The Kubernetes artifacts under `k8s/` and the
> Azure deployment work are deferred to the **next 14‑day sprint**
> (Sat 19 → Sat 3 Oct 2026). This sprint (Sat 5 → Sun 13 Sept) is a
> Kubernetes learning track: one concept per day, validated locally on
> Zorin with [`scripts/zorin-kind.sh`](scripts/zorin-kind.sh), journaled
> under [`docs/k8s/learn/`](docs/k8s/learn/README.md). No cloud, no ACR,
> no cost. After this block, the AKS push is an afternoon's work because
> you understand the manifests.

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
| [`docs/KUBERNETES-PLAN.md`](docs/KUBERNETES-PLAN.md) | Deployment design, constraints, and cost model. |
| [`docs/k8s/IMPLEMENTATION-PLAN.md`](docs/k8s/IMPLEMENTATION-PLAN.md) | Phased Kubernetes rollout. |
| [`docs/k8s/PHASE-0-CHECKLIST.md`](docs/k8s/PHASE-0-CHECKLIST.md) | Pre-cloud contract-fix checklist, with per-item status. |
| [`AGENTS.md`](AGENTS.md) | Conventions for AI coding agents working in this repository. |

Per-app documentation lives with the app: [`apps/angular-client/README.md`](apps/angular-client/README.md),
[`apps/mvc-web/README.md`](apps/mvc-web/README.md),
[`apps/web-api/README.md`](apps/web-api/README.md).

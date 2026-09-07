# MoviesApp Monorepo

MoviesApp is a secure, production-ready movie collection management system organized as a single monorepo containing three applications:

- Angular client (with optional SSR)
- ASP.NET Core MVC web app
- **OWASP Top 10 Compliant** ASP.NET Core Web API

This layout allows independent app development while keeping CI/CD, container orchestration, and contribution workflows centralized.

## Repository Layout

- `apps/angular-client`: Angular 21 client (with optional SSR)
- `apps/mvc-web`: ASP.NET Core MVC app
- `apps/web-api`: **OWASP Top 10 Compliant** ASP.NET Core Web API
- `.github/workflows`: Root-owned CI/CD workflows
- `docs`: Architecture, CI/CD, contributor, security, and deployment documentation
- `docker-compose.yml`: Local full-stack container orchestration
- `k8s/`: AKS manifests (Deployment, Service, ConfigMap, Secret, PVC, Ingress)

## Prerequisites

- Node.js 24.x (Angular app engines currently target 24.11.1)
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

### 4. Web API app (OWASP Top 10 Compliant)

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
- **The Angular client does not call the API.** `MovieService` returns hard‑coded seed data. The cluster therefore has **two** connected services (`mvc-web → web-api`) plus one standalone front end (`angular-client`), not three interconnected services. Wiring Angular to the live API is application feature work and is deliberately outside the deployment plan.
- **The Web API is internal.** Only `mvc-web` reaches it. CORS in `Program.cs` is hard‑coded to `http://localhost:4200` and a placeholder production origin; if the API is ever exposed publicly, that list must move to configuration first.

### Configuration and secrets

- **ConfigMap** `moviesapp-config` carries `ASPNETCORE_URLS`, `MoviesApi__BaseUrl`, `EnableHttpsRedirection`, and CORS origins (when applicable).
- **Secret** `moviesapp-secrets` carries `JwtSettings__SecretKey`. The committed placeholder in `appsettings.json` is removed — the app fails loudly at startup if the key is absent. The dev workflow still uses `dotnet user-secrets`.
- **Backup:** a daily `CronJob` copies `movies.db` from the PVC to a blob storage account using the cluster's workload identity.

### Security posture

- All three containers run as non‑root with `readOnlyRootFilesystem: true`, `allowPrivilegeEscalation: false`, and `capabilities.drop: [ALL]`.
- The ACR pull is via managed identity; no long‑lived registry credentials exist in the cluster.
- Web API image carries the same security middleware stack as production (`SecurityHeadersMiddleware`, `ExceptionHandlingMiddleware`, `RateLimitingMiddleware`, JWT + refresh tokens, GUID resource IDs, audit logging).
- Two known‑vulnerability packages were bumped before the first image push: `AutoMapper` 14.0.0 → 14.0.1+ ([GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x)) and `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 → 2.2.0+ ([GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q)).

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

## Contributor Workflow

Use the monorepo runbook for branch strategy, subtree imports, and gate checks:

- `docs/CONTRIBUTING-RUNBOOK.md`

## Related Documentation

- `docs/ARCHITECTURE.md`
- `docs/CI-CD.md`
- `docs/SECURITY-OVERVIEW.md`
- `docs/PROJECT-STATUS.md`
- `docs/KUBERNETES-PLAN.md` and `docs/k8s/IMPLEMENTATION-PLAN.md`
- `docs/k8s/INTERVIEW-PREP.md`
- `docs/moviesapp-diagram.mmd`
- `AGENTS.md`

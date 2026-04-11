# Contributor Runbook

This runbook defines how contributors should work in the MoviesApp monorepo.

## Branch and Ownership Model

- Keep changes scoped to one track or one app whenever possible.
- Use one branch per workstream. Example: `track-e-readme-runbook`.
- Keep root workflow files under `.github/workflows` owned by CI/CD changes only.

## Pull Request Expectations

Each PR should include:

- Scope summary (what changed, and why)
- Validation commands and outcomes
- Any known warnings or follow-up tasks

Keep PRs focused and small enough to review quickly.

## Local Validation Gates

Run the relevant gates before creating a PR.

### Angular client gate

```bash
cd apps/angular-client
npm ci
CI=true npm test
npm run build
```

### MVC gate

```bash
cd apps/mvc-web
dotnet restore MoviesWeb.sln
dotnet build MoviesWeb.sln --configuration Release --no-restore
dotnet test MoviesWeb.sln --configuration Release --no-build
```

### Web API gate

```bash
cd apps/web-api
dotnet restore MoviesAPI.sln
dotnet build MoviesAPI.sln --configuration Release --no-restore
dotnet test MoviesAPI.sln --configuration Release --no-build
```

### Docker gate

```bash
cd /path/to/MoviesApp
docker compose config
```

## Monorepo Import Workflow (git subtree)

Use this process for adding another repository while preserving history.

1. Create an import branch.
2. Add source repo as a temporary remote.
3. Fetch the source branch.
4. Import with subtree into `apps/<target-folder>`.
5. Run app-local validation gates.
6. Merge to main after review.
7. Remove temporary remote.

Example:

```bash
git checkout -b import/new-app
git remote add newapp-local /path/to/source-repo
git fetch newapp-local main
git subtree add --prefix=apps/new-app newapp-local main
# run validation gates
git checkout main
git merge --no-ff import/new-app
git remote remove newapp-local
```

## CI/CD Guardrails

- Prefer path-scoped changes to avoid unnecessary CI runs.
- Avoid editing app-owned code and root workflows in the same PR unless required.
- Keep deployment credentials in GitHub Environments and secrets, never in repository files.

## Rollback Guidance

- Use small PRs and clear commit messages for quick reverts.
- If a change affects shared automation, verify branch protections and required checks before merge.
- For import tracks, tag the merge commit so rollback points are explicit.

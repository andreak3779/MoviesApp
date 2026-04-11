# CI/CD Overview

The monorepo now uses root-owned GitHub Actions workflows under `.github/workflows/`.

## Workflows

- `validate-apps.yml`: Pull request and branch validation for `apps/angular-client`, `apps/mvc-web`, and `apps/web-api`.
- `container-images.yml`: Docker image build validation for changed apps, with a nightly full image build.
- `release-images.yml`: Manual or tag-based container image publishing to GHCR.

## Change Detection

Path filters are centralized in `.github/filters.yaml`.

- Changes under a single app folder run only that app's validation job.
- Changes to shared automation files such as `.github/workflows/**`, `docker-compose.yml`, `.env.example`, `README.md`, and `docs/**` run all validation jobs.
- Container builds are limited to app and container-related changes so docs-only updates do not spend Docker minutes.

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

## Release Images

Use the `Release Images` workflow to publish one app image or all app images to GHCR.

- Manual release: run the workflow and choose the target app.
- Tag release: push a tag matching `release-*`.
- Published image names:
  - `ghcr.io/<owner>/moviesapp-angular-client`
  - `ghcr.io/<owner>/moviesapp-mvc-web`
  - `ghcr.io/<owner>/moviesapp-web-api`

Deployment automation remains intentionally separate from image publishing so environment-specific secrets and approvals can be added later with GitHub Environments.

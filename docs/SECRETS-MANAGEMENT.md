# Secrets Management

This document describes how secrets, credentials, and other sensitive
values flow through the MoviesApp monorepo. It is the source of truth for
new contributors and agent prompts.

> **Background.** Runtime database files and cloud metadata files must never
> be committed. The `.gitignore`, pre-commit hooks and the gitleaks workflow
> enforce this.

## Rules of the road

1. **Never commit a real secret, password, API key, connection string, or
   user record to the repo.** The repo-level `.gitignore` blocks common
   patterns (`*.db`, `.env*`, `secrets.json`) and the pre-commit hook
   runs `gitleaks` on every commit.
2. **Never read a secret from `appsettings.json` in production.** Use
   environment variables (12-factor) or a Kubernetes `Secret`.
3. **Rotation is a normal operation.** If you suspect any value has been
   committed or printed, rotate it. Assume the value is compromised.
4. **Service-to-service identity.** Use workload identity (AKS) or
   managed identity (Azure) — no long-lived registry credentials in the
   cluster.

## What goes where

| Kind of secret | Dev | Production |
| --- | --- | --- |
| JWT signing key | `dotnet user-secrets set JwtSettings/SecretKey "$(openssl rand -base64 48)"` | Kubernetes `Secret` named `moviesapp-secrets` mounted as `JwtSettings__SecretKey` env var |
| DB connection string | `appsettings.Development.json` (local-only) | Kubernetes `Secret` named `moviesapp-secrets` mounted as `ConnectionStrings__DefaultConnection` env var |
| Microsoft / Azure credentials | `az login` interactive | AKS workload identity binding to ACR `AcrPull` |
| GHCR publish credentials | `GITHUB_TOKEN` (provided by Actions) | `GITHUB_TOKEN` (provided by Actions) — never a PAT |
| Email / SMTP credentials | not yet wired | not yet wired |

The placeholder text
`CHANGE_THIS_TO_A_SECURE_RANDOM_STRING_MIN_32_CHARS_IN_PRODUCTION` is
sentinel-only. `Program.cs` and `AuthService.cs` will refuse to start the
API if the configured secret still equals this string.

### Token handling in the MVC app (added 4.1.0)

The MVC web app authenticates against the same API and therefore holds the
same kind of secret material. Two surfaces are new since 4.0.0:

| Surface | Where | Notes |
| --- | --- | --- |
| Access + refresh tokens | `MoviesWeb/Services/MovieSession.cs`, `ApiAuthService.cs` | Held in server-side `ISession` (server memory / distributed cache), **never** in a cookie the browser can read. `MovieSession` refreshes the access token automatically. |
| Signed session cookie | ASP.NET Core cookie auth | The cookie carries a protected session id only. ASP.NET Core encrypts and signs it with the data-protection key ring. The bearer token itself is *not* in the cookie. |
| API base URL | `MoviesApi:BaseUrl` | Configuration, not a secret, but it must end in `/api/v1` — the MVC app refuses to start otherwise. |

Rules for this app specifically:

- **Never** put the bearer token into a cookie, a query string, or the HTML.
  The Razor views read from the service layer, never from `ISession` directly.
- The data-protection key ring is **not** configured for a shared store, so
  the session cookie is only valid for the instance that created it. That is
  acceptable for a single-replica local app; before scaling out, configure
  `AddDataProtection().PersistKeysToFileSystem(...)` or a Redis/KV backing
  store and set the same key ring across replicas.
- The MVC app does **not** have its own JWT signing key. It never signs or
  verifies tokens; the API is the only issuer and verifier.

## Detection

The following tools run automatically:

- **Local** — `gitleaks` and `detect-secrets` run as pre-commit hooks
  defined in `.pre-commit-config.yaml`. Install with `pre-commit install`.
- **Pull request** — `.github/workflows/security.yml` runs `gitleaks`
  against the full history, `npm audit --audit-level=high` for the
  Angular client, and `dotnet list package --vulnerable` for the .NET
  solutions.
- **Schedule** — the same workflow runs weekly against `main`.

If any of these fail, the build is red. Fix forward — do not edit the
workflow just to make it pass.

## Reporting a leak

If you suspect a secret has been committed:

1. Stop. Do not push further changes until the secret is rotated.
2. Rotate the secret in the upstream system (e.g. revoke the JWT key, the
   PAT, the connection string).
3. Open an issue tagging `security`.
4. Coordinate a `git filter-repo` rewrite with maintainers (any history
   rewrite is a coordinated force-push).

## Tooling references

- [OWASP API Security Top 10 (2023)](https://owasp.org/API-Security/editions/2023/en/0x11-t10/)
- [gitleaks](https://github.com/gitleaks/gitleaks)
- [Azure Workload Identity for AKS](https://learn.microsoft.com/azure/aks/workload-identity-overview)

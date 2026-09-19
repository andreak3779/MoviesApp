# Secrets Management

This document describes how secrets, credentials, and other sensitive
values flow through the MoviesApp monorepo. It is the source of truth for
new contributors and agent prompts.

> **Background.** On **2026-09-14** a security audit found that two
> SQLite databases and an Azure metadata file had been committed to git
> history. See [SECURITY-INCIDENT-2026-09-14.md](SECURITY-INCIDENT-2026-09-14.md).

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
3. Open an issue tagging `security`. Reference
   [SECURITY-INCIDENT-2026-09-14.md](SECURITY-INCIDENT-2026-09-14.md).
4. Coordinate a `git filter-repo` rewrite with maintainers (any history
   rewrite is a coordinated force-push).

## Tooling references

- [OWASP API Security Top 10 (2023)](https://owasp.org/API-Security/editions/2023/en/0x11-t10/)
- [gitleaks](https://github.com/gitleaks/gitleaks)
- [Azure Workload Identity for AKS](https://learn.microsoft.com/azure/aks/workload-identity-overview)

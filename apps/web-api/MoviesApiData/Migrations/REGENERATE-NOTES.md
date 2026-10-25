# Migration history

This folder contains a single `InitialSchema` migration that creates the
full database schema from scratch. It is the canonical schema for the
MoviesApp API (4.0.0).

## Why a single migration?

Prior to 4.0.0, this folder held three migrations (`AddIdentityAndOwnership`,
`AddAuditLogging`, `ChangeMovieIdToGuid`) that were committed while the
domain types still lived in the `MoviesApi.Models` namespace. The models
later moved to `MoviesApiDomain.Models`, but the historical migration
designers were never regenerated. As a result `dotnet ef migrations add`
could not produce a clean diff between the prior designers and the
current model: every regeneration either produced an empty migration or a
full-schema rewrite.

To deliver a working, clean migration for 4.0.0, the migration history was
reset to a single `InitialSchema` migration that creates the entire schema
(AspNet tables, lookups, `Movies`, junctions, `Formats`, `MovieFormats`,
seed data). Existing databases that already applied the prior three
migrations will record them as orphaned in `__EFMigrationsHistory`; the
recommended cleanup for those databases is to drop and recreate the DB.

## Verifying the migration

```bash
cd apps/web-api/MoviesApi
dotnet tool run dotnet-ef migrations list \
  --project ../MoviesApiData/MoviesApiData.csproj \
  --startup-project ./MoviesApi.csproj
dotnet tool run dotnet-ef database update \
  --project ../MoviesApiData/MoviesApiData.csproj \
  --startup-project ./MoviesApi.csproj
```

## Default JWT secret

`Program.cs` fails fast at startup if `JwtSettings:SecretKey` matches the
placeholder string. Local development must set a real 32+ byte secret via
user-secrets before any EF command can boot the host:

```bash
cd apps/web-api/MoviesApi
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 48)"
```

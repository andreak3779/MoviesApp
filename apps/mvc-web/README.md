# MoviesWeb

[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE) [![.NET](https://img.shields.io/badge/.NET-net10.0-blue.svg)](https://dotnet.microsoft.com/) [![Editor: VS Code](https://img.shields.io/badge/Editor-VS%20Code-blue?logo=visual-studio-code)](https://code.visualstudio.com) [![OS: Zorin OS](https://img.shields.io/badge/OS-Zorin%20OS-5078A0)](https://zorinos.com)

MoviesWeb is a small ASP.NET Core web application for browsing and managing movie data. The project solution and the web app are included in this repository.

**Skills demonstrated:** ASP.NET Core, MVC, Razor views, C#, web application architecture

**Project:** MoviesWeb

MoviesWeb is the server-rendered front end for the MoviesApp Web API. It signs
in against the API's `/api/v1/auth` endpoints, stores the issued JWT in the
server-side session, and attaches it to every upstream request. The pages
follow the **MoviesApp UI Mockup**: a sidebar shell, a card grid for the
collection, an add / edit form, a details page and a sign-in screen.

## How it talks to the Web API

| Concern | Detail |
| --- | --- |
| Base address | `MoviesApi:BaseUrl` — must include the `/api/v1` prefix, e.g. `http://localhost:8081/api/v1` |
| Movie routes | `/api/v1/Movie` (singular — the API's controller is `MovieController`) |
| Lookup routes | `/api/v1/FilmGenres`, `/api/v1/MediaTypes`, `/api/v1/Formats` |
| Auth routes | `/api/v1/Auth/login`, `/register`, `/refresh`, `/logout` |
| Identifiers | GUIDs, not integers |
| Wire format | camelCase JSON; the list endpoint returns a paged envelope (`items`, `totalCount`, `hasNext`, ...) |
| Auth | `Authorization: Bearer <token>` attached by `ApiTokenHandler` from the session |
| Sessions | ASP.NET Core session for the API tokens, cookie auth for the browser |
| Sign-out | Revokes the refresh token on the API before the local session is cleared |

### Running against the API

```bash
# 1. Start the Web API (it needs a real JWT secret)
cd ../web-api/MoviesApi
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 48)"
dotnet run

# 2. Start this app pointed at it
cd ../../mvc-web
MoviesApi__BaseUrl=http://localhost:8081/api/v1 dotnet run --project MoviesWeb/MoviesWeb.csproj
```

Then sign in at `/Account/Login` with an account registered through
`POST /api/v1/auth/register`. Note that the API requires passwords containing
an uppercase letter, a lowercase letter, a digit **and** a symbol.

## Prerequisites

- .NET SDK 10.0 or later (install from https://dotnet.microsoft.com)
- A code editor (VS Code, Visual Studio, Rider)

## Build and run

From the repository root you can build and run the web app with the .NET CLI:

```bash
dotnet build MoviesWeb/MoviesWeb.csproj
dotnet run --project MoviesWeb/MoviesWeb.csproj
```

By default the app will use settings from `appsettings.json` and `appsettings.Development.json` when running in Development.

To publish a release build:

```bash
dotnet publish MoviesWeb/MoviesWeb.csproj -c Release -o out
```

## Project structure (key files)

- MoviesWeb/MoviesWeb.csproj - project file
- MoviesWeb/Program.cs - application entry (DI, session, cookie auth)
- MoviesWeb/Controllers/AccountController.cs - sign in, register and sign out against the API
- MoviesWeb/Controllers/MoviesController.cs - movie list, add / edit form and details pages
- MoviesWeb/Controllers/HomeController.cs - home, privacy and error pages
- MoviesWeb/Services/MoviesService.cs - HTTP client for `/api/v1/Movie`
- MoviesWeb/Services/ApiAuthService.cs - HTTP client for `/api/v1/auth`
- MoviesWeb/Services/ApiTokenHandler.cs - attaches the bearer token
- MoviesWeb/Services/MovieSession.cs - stores and refreshes the API tokens
- MoviesWeb/Models/ - models mirroring the API's DTOs
- MoviesWeb/ViewModels/ - list, form and tile view models
- MoviesWeb/Views - Razor views (`Movies`, `Account`, `Home`, `Shared`)
- MoviesWeb/wwwroot/css/site.css - the mockup's design tokens and layout

## Development notes

- The project targets `net10.0` (check `obj/` and `bin/` outputs). Ensure your SDK matches.
- Configuration files: `appsettings.json` and `appsettings.Development.json`.

## Tests

The `MoviesWeb.Tests` project (xUnit + Moq + `WebApplicationFactory`) covers
the API clients, the controllers, the session and the Razor views:

```bash
dotnet test MoviesWeb.sln
```

To reproduce the coverage report CI publishes:

```bash
dotnet test MoviesWeb.sln --configuration Release \
  --collect:"XPlat Code Coverage" \
  --settings MoviesWeb.Tests/coverlet.runsettings \
  --results-directory ./coverage
```

The repo enforces a **70% line-coverage gate** per app
(`scripts/check-coverage.py`); the MVC app currently sits at **93.42%**.

## Contributing

- Open an issue or submit a pull request. Keep changes small and focused.

## License

No license file is included. Add a `LICENSE` file if you intend to publish under a specific license.

## Acknowledgments

- Development assistance provided by GitHub Copilot and GPT-5 mini.


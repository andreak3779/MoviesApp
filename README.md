# MoviesWeb

[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE) [![.NET](https://img.shields.io/badge/.NET-net10.0-blue.svg)](https://dotnet.microsoft.com/) [![Editor: VS Code](https://img.shields.io/badge/Editor-VS%20Code-blue?logo=visual-studio-code)](https://code.visualstudio.com) [![OS: Zorin OS](https://img.shields.io/badge/OS-Zorin%20OS-5078A0)](https://zorinos.com)

MoviesWeb is a small ASP.NET Core web application (Razor Pages / MVC mixed layout) for browsing and managing movie data. The project solution and the web app are included in this repository.

**Skills demonstrated:** ASP.NET Core, Razor Pages, MVC, C#, web application architecture

*Note: This is a demonstration project and not intended for production use.*

**Project:** MoviesWeb

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
- MoviesWeb/Program.cs - application entry
- MoviesWeb/Controllers/MovieController.cs - controller for movie pages
- MoviesWeb/Models/ - domain models (`Movie.cs`, `User.cs`, etc.)
- MoviesWeb/Pages, MoviesWeb/Views - Razor views and pages
- MoviesWeb/wwwroot - static assets (css, js, lib)

## Development notes

- The project targets `net10.0` (check `obj/` and `bin/` outputs). Ensure your SDK matches.
- Configuration files: `appsettings.json` and `appsettings.Development.json`.

## Tests

No automated tests are included in the repository. Consider adding an xUnit or NUnit test project for unit/integration tests.

## Contributing

- Open an issue or submit a pull request. Keep changes small and focused.

## License

No license file is included. Add a `LICENSE` file if you intend to publish under a specific license.

## Acknowledgments

- Development assistance provided by GitHub Copilot and GPT-5 mini.


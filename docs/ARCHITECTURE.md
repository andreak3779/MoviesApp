# Architecture & Diagram

This document contains the project's architecture diagram and instructions to render it.

Mermaid source (from `docs/moviesapp-diagram.mmd`):

```mermaid
graph LR
  subgraph Client
    App[App (src/app/app.ts)]
    Header[MovieHeader]
    List[MoviesList]
    Watch[MovieWatchList]
  end

  subgraph NgRx
    Store[NgRx Store / Effects]
  end

  Service[MovieService<br/>(movies-app/src/services/movie-service.ts)]
  WatchService[MovieWatchListService]
  Models[Models<br/>(Movie, MediaType, MovieFormat)]
  Server[SSR Server<br/>(src/server.ts, main.server.ts)]
  Public[Public Assets<br/>(public/)]

  App --> Header
  App --> List
  App --> Watch

  Header -->|dispatch| Store
  List -->|dispatch| Store
  Watch -->|dispatch| Store

  Store -->|effects -> calls| Service
  Store -->|effects -> calls| WatchService

  Service --> Models
  WatchService --> Models

  Server --> Service
  App --> Public
```

Render instructions

- Option A — render locally with Mermaid CLI (recommended for PNG/SVG output):

```bash
# Install (optional global)
npm install -g @mermaid-js/mermaid-cli

# Render to SVG
mmdc -i docs/moviesapp-diagram.mmd -o docs/moviesapp-diagram.svg
```

- Option B — use npx (no global install):

```bash
npx @mermaid-js/mermaid-cli -i docs/moviesapp-diagram.mmd -o docs/moviesapp-diagram.svg
```

Notes

- The Mermaid block above is also viewable in many editors that support Mermaid previews (VS Code Mermaid Preview, Obsidian, GitHub with mermaid enabled).

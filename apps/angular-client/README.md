# MoviesApp Angular Client

The primary front end for MoviesApp: an Angular 22 standalone-component
application with NgRx 22 state management, optional SSR, and a JWT-secured
call to the Web API.

## Prerequisites

- Node.js 24.x (`package.json` declares `node: ^24.0.0`)
- npm

## Getting started

```bash
npm ci
npm start        # ng serve -> http://localhost:4200
```

Useful commands:

```bash
npm test         # Vitest via ng test
npm run build    # production build into dist/
npm run watch    # development build in watch mode
npm run serve:ssr:movies-app   # serve the built SSR bundle
```

## Routes

| Route | Purpose | Guarded |
| --- | --- | --- |
| `/login` | Sign in | No — lives outside the app shell |
| `/movies` | Movie list: search, filters, sort, paging, delete | Yes |
| `/movies/new` | Add a movie | Yes |
| `/movies/:id/edit` | Edit a movie | Yes |
| `/genres`, `/genres/new`, `/genres/:id/edit` | Genre lookups | Yes |
| `/media-types`, `/media-types/new`, `/media-types/:id/edit` | Media type lookups | Yes |
| `/formats`, `/formats/new`, `/formats/:id/edit` | Format lookups | Yes |
| `**` | Redirects to `/movies` | — |

`authGuard` is a `canMatch` guard: it redirects to `/login` with a `returnUrl`
query parameter when no token is present.

## Source layout

```
src/app/
├── app.ts, app.config.ts, app.routes.ts, app.routes.server.ts
├── core/
│   ├── api-config.ts                  API_BASE_URL injection token
│   ├── auth/login.component.ts
│   ├── guards/auth.guard.ts
│   ├── interceptors/auth.interceptor.ts     attaches the bearer token
│   ├── interceptors/error.interceptor.ts    handles 401 -> /login
│   └── services/
│       ├── auth.service.ts            token storage + login/refresh/logout
│       └── lookup.service.ts          cached read-only lookup lists
├── features/
│   ├── chrome/sidebar.component.ts
│   ├── movies/movies-list.component.ts, movie-form.component.ts
│   ├── lookups/lookup-list.component.ts, lookup-form.component.ts,
│   │           lookup.service.ts      (LookupAdminService - writes)
│   └── shared/delete-confirmation.component.ts
├── models/                            movie.ts, film-genre.ts, media-type.ts, format.ts
├── services/movie-service.ts
└── store/movies/                      actions, reducer, selectors, effects
```

> **Two lookup services.** `core/services/lookup.service.ts` is the
> **read-only cached** service (signals) used by the list and form components.
> `features/lookups/lookup.service.ts` exports `LookupAdminService`, the
> **write** client. They are easy to confuse.

## State

One NgRx feature slice, `movies`, built on `createEntityAdapter`:

| Action | Effect |
| --- | --- |
| `loadMovies` | `MovieService.search(query)` -> `loadMoviesSuccess` / `loadMoviesFailure` |
| `addMovie` | `MovieService.create(payload)` -> `addMovieSuccess` / `addMovieFailure` |
| `updateMovie` | `MovieService.update(id, payload)` -> `updateMovieSuccess` / `updateMovieFailure` |
| `removeMovie` | `MovieService.remove(id)` -> `removeMovieSuccess` / `removeMovieFailure` |

Selectors: `selectAllMovies`, `selectMovieEntities`, `selectMoviesTotal`,
`selectMoviesLoading`, `selectMoviesError`, `selectSelectedMovie`,
`selectFavouriteMovies`.

## API configuration

`API_BASE_URL` (`core/api-config.ts`) defaults to
`http://localhost:8081/api/v1` — the dev port used by `docker-compose.yml`.
Override it by providing the token in `app.config.ts`:

```ts
{ provide: API_BASE_URL, useValue: 'https://api.example.com/api/v1' }
```

The base URL **must include the `/api/v1` prefix**. The API routes movies at
`/api/v1/Movie` (singular — the controller is `MovieController`); lookup
routes are `/api/v1/filmGenres`, `/api/v1/mediaTypes`, `/api/v1/formats`.

## Authentication

`AuthService` signs in against `/api/v1/auth/login` and stores the access and
refresh tokens in `sessionStorage` under `moviesapp.accessToken` and
`moviesapp.refreshToken`. `authInterceptor` attaches
`Authorization: Bearer <token>` to every request except the auth endpoints.
`errorInterceptor` clears the stored tokens and redirects to `/login` on a 401.

> **Known limitation.** The access token lives 15 minutes and nothing calls
> `AuthService.refresh()` — there is no 401-retry. A session therefore ends
> after 15 minutes until the user signs in again. The MVC client's
> `MovieSession` implements automatic refresh and is the reference approach.

## Tests

Vitest, run through `ng test`. Four spec files, **26 tests**:

| Spec | Covers |
| --- | --- |
| `app.spec.ts` | Shell renders, sidebar brand |
| `store/movies/movies.reducer.spec.ts` | Every reducer transition |
| `store/movies/movies.selectors.spec.ts` | Derived state |
| `store/movies/movies.effects.spec.ts` | Effects against a stubbed service |

> There are no component-level tests yet. The store is covered; the
> components, the interceptors and the guard are not.

## Build

```bash
npm run build
```

Production bundles go to `dist/movies-app/`. The SSR server bundle can be
served with `npm run serve:ssr:movies-app`.

## Further reading

- [Architecture](../../docs/ARCHITECTURE.md)
- [Project status](../../docs/PROJECT-STATUS.md)
- [Security overview](../../docs/SECURITY-OVERVIEW.md)

# NgRx Guidelines for Agents

## Presence
- The project includes `@ngrx/store` and `@ngrx/store-devtools` (see `movies-app/package.json`). Follow existing slices and naming conventions if you modify global state.

## Patterns
- Use `createAction`, `createReducer`, `createSelector` and keep reducer logic pure.
- Keep actions small and focused (e.g., `loadMovies`, `loadMoviesSuccess`, `loadMoviesFailure`).
- Prefer selectors for derived data; avoid putting derived logic in components.

## Effects & side-effects
- If adding async flows, implement `@ngrx/effects` to isolate side-effects (HTTP calls, navigation). Mock effects in unit tests.

## Tests
- Unit test reducers and selectors directly. For effects, use `provideMockActions` and test observable flows.

## When not to use NgRx
- For simple local state inside a small feature, prefer component-level state or services. Use NgRx when cross-component coordination or caching is needed.

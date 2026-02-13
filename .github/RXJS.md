# RxJS Guidelines for Agents

## Version
- This project uses `rxjs` ~7.8.0 (see `movies-app/package.json`).

## Recommended Patterns
- Services should expose cold `Observable` streams rather than resolving/promising values. See `movies-app/src/app/services/movie-service.ts` for examples.
- Prefer pipeable operators and functional composition (`pipe(map(...), catchError(...))`).
- Avoid direct `subscribe()` calls in services; transform streams and let components subscribe or use the `async` pipe.

## Component subscriptions
- Use `async` pipe in templates instead of manual subscription when possible.
- For manual subscriptions in components, use a `takeUntil`/`Subject` pattern to unsubscribe on `ngOnDestroy`.

## Error handling & side effects
- Centralize error handling in services or effects (NgRx) and return observable errors that callers can handle.
- Keep side effects isolated (do not mutate input objects inside `map`).

## Testing
- Use `marbles` or synchronous schedulers for deterministic RxJS tests when testing complex streams.

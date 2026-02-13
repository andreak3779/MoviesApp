# Angular Guidelines for Agents

## What this repo uses
- Angular CLI generated app (Angular v21.x). Client entry: `movies-app/src/main.ts`. SSR server entry: `movies-app/src/main.server.ts` and `movies-app/server.ts`.
- Project schematics and style defaults are in `movies-app/angular.json`.

## Code & style
- Styles use SCSS; when generating components prefer `--style=scss` or rely on `angular.json` schematics.
- Follow Prettier settings in `movies-app/package.json` (printWidth: 100, singleQuote: true). HTML uses the `angular` parser.

## Commands
- Install: `cd movies-app && npm ci`
- Dev server: `npm start` (runs `ng serve`).
- Build: `npm run build`.
- Watch: `npm run watch`.
- Tests: `npm test` (Vitest via `ng test`).

## SSR notes
- SSR uses `@angular/ssr` and Express in `movies-app/server.ts`. When updating server/SSR code ensure `package.json` `serve:ssr:movies-app` matches the output path in `dist/`.

## Patterns to follow
- Keep components small and focused. Prefer input/output bindings and services for shared logic.
- Centralize API calls in services under `movies-app/src/app/services/` and keep them testable (return observables).
- Use the `async` pipe in templates instead of manual subscribes when possible.
- Redux pattern should be followed.

## Tests and CI
- Unit tests live next to code with `*.spec.ts` names. Run `npm test` after changes that affect behavior.

If you need scaffolding snippets or CI steps for Angular builds, tell me which target (dev/prod/SSR) and I'll add them.

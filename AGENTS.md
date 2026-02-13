# Agent Guidelines (repo-level)

Purpose
- Provide succinct, actionable rules for AI coding agents working in this repository.

How to operate
- Start every non-trivial task with a short plan and track progress using the repo's todo mechanism.
- Run repository-local build and tests after any behavior-changing edits (see "Build & Test" below).
- Keep changes minimal and localized; prefer updating existing files rather than broad refactors.

Build & Test (commands run from the repo root or `movies-app/`)
- Install deps: `cd movies-app && npm ci`
- Start dev server: `cd movies-app && npm start`
- Build: `cd movies-app && npm run build`
- Tests: `cd movies-app && npm test`

Repository conventions to respect
- Formatting: follow Prettier settings in `movies-app/package.json` (printWidth: 100, singleQuote: true). HTML uses the `angular` parser.
- Styles: SCSS. Use `--style=scss` or rely on `movies-app/angular.json` schematics.
- Services: keep small and testable (see `movies-app/src/app/services/movie-service.ts`).
- Models: keep in `movies-app/src/app/models/` (see `movies-app/src/app/models/movie.ts`).

SSR & server notes
- Server-side rendering uses Express and Angular SSR. Server wiring is in `movies-app/server.ts` and `movies-app/main.server.ts`.
- When modifying SSR code ensure `package.json` script `serve:ssr:movies-app` matches the built artifact path.

Security
- Never write secrets or credentials into the repository. Use environment variables and document required vars in PRs.

When unsure
- If intent is unclear or changes are large, open an issue describing the proposed approach and wait for maintainer confirmation.

Related guidance files
- See `/.github/copilot-instructions.md`, `/.github/ANGULAR.md`, `/.github/RXJS.md`, and `/.github/NGRX.md` for more targeted rules.

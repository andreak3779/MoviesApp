# Mockoon (Mock API) Guidance

This project includes a Mockoon environment to mock backend API calls for development and testing.

Where to find the environment
- `movies-app/mockoon/movie-api.json` — ready-to-import Mockoon environment (example `/movies` endpoints).
- `movies-app/mockoon/README.md` — quick run notes.

Run options
- Official CLI (when available):
  - Install (optional global): `npm i -g @mockoon/cli` or add `@mockoon/cli` as a devDependency.
  - Start: `mockoon-cli start --data movies-app/mockoon/movie-api.json --port 3001`
- Local Node fallback (included):
  - Run lightweight mock server: `node movies-app/scripts/mock-server.js`
  - The Node server reads the Mockoon JSON and serves the example endpoints on port `3001`.

How to use in development
- Point your frontend dev config or service base URL to `http://localhost:3001` for API calls.
- Import `movie-api.json` into the Mockoon desktop app if you want to visually edit routes, add latency, or create templated responses.

Recommended practices
- Keep the mock environment minimal and focused on endpoints your app consumes (avoid syncing with full production schema unless needed).
- Use templated responses and fixed delays in the Mockoon GUI to simulate real-world latency and varied responses for robust UI testing.
- For authentication flows, add a route that validates tokens or returns test tokens for dev use; never include real credentials in the file.

Testing tips
- Use the mock server during component/integration tests by starting it in CI or tests' setup step (headless CLI preferred).
- When using the Node fallback, make sure tests start/stop the mock process to avoid port conflicts.

Notes
- The repo includes `movies-app/scripts/mock-server.js` as a simple fallback so contributors can run mocks without installing Mockoon CLI.
- If you prefer the official Mockoon tooling, install `@mockoon/cli` and update `movies-app/package.json` scripts accordingly.

If you want, I can:
- Add more example routes (search, filter, authentication), or
- Add a CI step to start the mock server during tests.

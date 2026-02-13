# Mockoon Movie API

This folder contains a Mockoon environment used to mock backend API calls during development and tests.

Files:
- `movie-api.json` — Mockoon environment with example `/movies` endpoints.

Run locally using the Mockoon CLI (installed as a dev dependency):

```bash
cd movies-app
npm ci
npm run mock-api
```

The mock server will listen on port `3001` by default. You can import `movie-api.json` into the Mockoon GUI if you prefer a visual editor.

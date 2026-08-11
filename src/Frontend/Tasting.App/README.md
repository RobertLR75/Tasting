# Tasting participant app

The participant-facing frontend is a separate Ionic React + TypeScript app. It uses the Tasting API in the authenticated user security context and does not contain admin capabilities.

## Run locally

```bash
npm install
VITE_API_BASE_URL=http://localhost:5000 npm run dev
```

The API base URL defaults to `http://localhost:5000` when `VITE_API_BASE_URL` is not set.

## Verify

```bash
npm run build
npm test
npx playwright install chromium
npm run test:e2e
```

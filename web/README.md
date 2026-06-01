# LogsheetXtractor Web

React + TypeScript + Vite frontend for LogsheetXtractor.

## Prerequisites

- Node.js `>=20.19.0 <21` or `>=22.12.0`
- pnpm `10.15.1` or newer
- Running LogsheetXtractor backend, either through Docker Compose or local .NET run

## Setup

From `web/`:

```bash
pnpm install
```

## Local Development

Start the frontend dev server:

```bash
pnpm dev
```

By default Vite uses the port configured in `.env`:

```bash
VITE_PORT=5226
```

The Docker deployment serves the frontend at `http://localhost:3000` and proxies backend requests through Nginx.

## Backend Connection

In Docker, API and SignalR traffic is proxied by `nginx.conf.template`:

- `/api/*` -> backend API
- `/hubs/*` -> SignalR hubs

For local development, keep the backend running and use the frontend's configured API behavior from the existing Vite setup.

## Scripts

```bash
pnpm dev              # Start Vite development server
pnpm build            # Type-check and build production assets
pnpm lint             # Run ESLint
pnpm test             # Run Vitest unit tests
pnpm test:coverage    # Run Vitest with coverage
pnpm test:e2e         # Run Playwright end-to-end tests
pnpm preview          # Preview production build locally
```

Install Playwright browser dependencies before the first E2E run:

```bash
pnpm test:e2e:install
```

## Project Structure

Important directories:

- `src/modules/*`: feature modules, including API hooks, schemas, pages, actions, and local components
- `src/components`: shared UI components
- `src/lib`: shared helpers such as API error handling
- `src/schema.ts`: shared Zod primitives
- `src/i18n`: localization messages

Frontend server communication should go through feature-local React Query hooks in `modules/<feature>/api.ts`. Request and response shapes should be validated with Zod schemas from the same module.

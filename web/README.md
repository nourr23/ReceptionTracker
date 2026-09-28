# ReceptionTracker – Web

React + TypeScript front-end used by the warehouse worker to receive a supplier order.

## Getting started

The .NET API must be running on `http://localhost:5143` (see the root README).

```bash
npm install
npm run dev        # http://localhost:5173 – /api is proxied to the .NET API
```

## Scripts

| Script                 | Purpose                                                         |
| ---------------------- | --------------------------------------------------------------- |
| `npm run dev`          | Dev server with hot reload                                      |
| `npm run build`        | Type-check and production build (`dist/`)                       |
| `npm test`             | Unit and UI tests (Vitest + Testing Library + MSW)              |
| `npm run lint`         | ESLint (type-aware rules)                                       |
| `npm run format`       | Prettier                                                        |
| `npm run generate:api` | Regenerate `src/api/schema.d.ts` from the running API's OpenAPI |

## Structure

```
src/
  api/                  HTTP client, error handling, generated API types
  components/           Generic UI (tri-state checkbox, alert, spinner…)
  features/reception/   The reception feature: queries, status rules, order tree components
  hooks/                Generic hooks
  test/                 Test setup, fake API (MSW), render helper
```

## Design choices

- **API types are generated** from the OpenAPI document (`openapi-typescript` + `openapi-fetch`):
  a change in the API contract becomes a TypeScript error, never a runtime surprise.
- **Server state lives in TanStack Query.** Every update returns the whole refreshed order,
  which replaces the cached one: parent statuses and progress are always computed by the server,
  the business rules are not duplicated in the UI.
- **Updates of an order run one at a time** (mutation `scope`), so a slow response can never
  overwrite a newer state. The clicked checkbox shows its new value immediately while in flight.
- **Light interface:** pallets show their cartons, products are shown on demand, each level shows a
  "x / y" counter and a status, and received elements can be hidden.
- **Accessible:** native checkboxes (indeterminate = "mixed"), labels as click targets,
  `aria-expanded` disclosures, `role="progressbar"`, `role="alert"` errors, keyboard focus styles.
- **Tests from the user's point of view:** the whole app runs against a fake API (MSW) that
  follows the real contract.

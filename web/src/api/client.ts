import createClient from 'openapi-fetch'
import { ApiError } from './errors'
import type { paths } from './schema'

/**
 * Typed HTTP client generated from the API's OpenAPI document (see `npm run generate:api`):
 * URLs, path parameters, bodies and responses are all checked by TypeScript.
 */
export const apiClient = createClient<paths>({
  // Same origin: in development Vite proxies /api to the .NET API.
  baseUrl: window.location.origin,
  // Resolved at call time (not captured once) so test tools like MSW can intercept requests.
  fetch: (request) => globalThis.fetch(request),
})

type ApiResult<T> = { data?: T; error?: unknown; response: Response }

/** Returns the response body, or throws an {@link ApiError} built from the ProblemDetails. */
export async function unwrap<T>(request: Promise<ApiResult<T>>): Promise<T> {
  const { data, error, response } = await request
  if (!response.ok || data === undefined) {
    throw ApiError.fromResponse(response.status, error)
  }
  return data
}

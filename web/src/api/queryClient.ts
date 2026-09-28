import { QueryClient } from '@tanstack/react-query'
import { ApiError } from './errors'

export function createQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // A client error (404, 400…) will fail again: only network/server errors are retried.
        retry: (failureCount, error) =>
          !(error instanceof ApiError && error.status < 500) && failureCount < 2,
      },
    },
  })
}

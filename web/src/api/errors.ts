/** Subset of an RFC 9457 ProblemDetails body, plus the API's own error `code`. */
interface ProblemDetails {
  title?: string | null
  detail?: string | null
  code?: string
}

function isProblemDetails(value: unknown): value is ProblemDetails {
  return typeof value === 'object' && value !== null
}

export class ApiError extends Error {
  readonly status: number
  readonly code: string | undefined

  constructor(status: number, message: string, code?: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }

  static fromResponse(status: number, body: unknown): ApiError {
    const problem = isProblemDetails(body) ? body : {}
    const message = problem.detail ?? problem.title ?? `La requête a échoué (HTTP ${status}).`
    return new ApiError(status, message, problem.code)
  }
}

/** A message that can be shown to the user, whatever went wrong. */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message
  }
  // fetch() rejects with a TypeError when the server can't be reached.
  if (error instanceof TypeError) {
    return 'Impossible de contacter le serveur. Vérifiez que l’API est démarrée.'
  }
  return 'Une erreur inattendue est survenue.'
}

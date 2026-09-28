import { describe, expect, it } from 'vitest'
import { ApiError, getErrorMessage } from './errors'

describe('ApiError.fromResponse', () => {
  it('uses the ProblemDetails detail and code', () => {
    const error = ApiError.fromResponse(404, {
      title: 'Not Found',
      detail: "Order 'X' was not found.",
      code: 'Order.NotFound',
    })

    expect(error.status).toBe(404)
    expect(error.message).toBe("Order 'X' was not found.")
    expect(error.code).toBe('Order.NotFound')
  })

  it('falls back to a generic message without a body', () => {
    expect(ApiError.fromResponse(502, undefined).message).toBe('La requête a échoué (HTTP 502).')
  })
})

describe('getErrorMessage', () => {
  it('explains network failures', () => {
    expect(getErrorMessage(new TypeError('Failed to fetch'))).toMatch(/Impossible de contacter/)
  })

  it('never shows technical details of unknown errors', () => {
    expect(getErrorMessage(new Error('NullReferenceException at line 42'))).toBe(
      'Une erreur inattendue est survenue.',
    )
  })
})

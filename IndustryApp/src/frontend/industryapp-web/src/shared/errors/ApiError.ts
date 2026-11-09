/**
 * Axios hatasının ekrana uygun hali. message her zaman Türkçedir (errorMessages); fieldErrors, 400
 * VALIDATION_FAILED yanıtındaki alan hatalarıdır (camelCase alan adı → mesajlar).
 */
export class ApiError extends Error {
  readonly status: number
  readonly code: string | null
  readonly fieldErrors: Record<string, string[]>

  constructor(message: string, status: number, code: string | null, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.fieldErrors = fieldErrors
  }
}

export const isApiError = (error: unknown): error is ApiError => error instanceof ApiError

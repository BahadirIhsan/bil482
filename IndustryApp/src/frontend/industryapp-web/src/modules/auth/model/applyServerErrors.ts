import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import { isApiError } from '../../../shared/errors'

/**
 * 400 VALIDATION_FAILED yanıtındaki alan hatalarını forma işler; formda karşılığı olan alan varsa true döner.
 * Sunucu alan adları camelCase'tir ve form alanlarıyla aynıdır.
 */
export function applyServerErrors<T extends FieldValues>(error: unknown, fields: readonly Path<T>[], setError: UseFormSetError<T>): boolean {
  if (!isApiError(error)) return false
  let applied = false
  for (const [field, messages] of Object.entries(error.fieldErrors)) {
    const match = fields.find((f) => f === field)
    if (match && messages.length > 0) {
      setError(match, { type: 'server', message: messages[0] })
      applied = true
    }
  }
  return applied
}

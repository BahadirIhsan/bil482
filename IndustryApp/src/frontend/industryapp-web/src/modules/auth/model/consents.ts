import type { LegalDocument } from '../../catalog/model/types'

/** Tüm zorunlu sözleşmeler işaretli mi. */
export const allAccepted = (documents: LegalDocument[] | undefined, accepted: number[]): boolean =>
  Boolean(documents && documents.length > 0 && documents.every((d) => accepted.includes(d.id)))

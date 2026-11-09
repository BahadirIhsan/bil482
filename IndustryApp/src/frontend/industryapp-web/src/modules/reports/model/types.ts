// UC-04 · Rapor detayı tipleri (GET /api/reports/{id}, endpoint 18). JSON camelCase, enum'lar metin.

export type ReportStatus = 'Open' | 'OfferSelected' | 'Completed' | 'Cancelled' | 'Expired'

export type DamageSeverity = 'Light' | 'Medium' | 'Heavy'

export interface ReportDamageItem {
  damageCategoryId: number
  categoryName: string
  severity: DamageSeverity
  note: string | null
}

export interface ReportDetail {
  id: string
  status: ReportStatus
  city: string
  district: string | null
  addressText: string | null
  description: string | null
  vehicleTypeName: string
  createdAt: string
  expiresAt: string
  selectedOfferId: string | null
  offerSelectedAt: string | null
  undoDeadline: string | null
  completedAt: string | null
  cancelledAt: string | null
  damageItems: ReportDamageItem[]
  photoIds: string[]
}

/** Ham enum değeri ekranda gösterilmez (NFR-U2). */
export const reportStatusLabels: Record<ReportStatus, string> = {
  Open: 'Teklif bekliyor',
  OfferSelected: 'Teklif seçildi',
  Completed: 'Tamamlandı',
  Cancelled: 'İptal edildi',
  Expired: 'Süresi doldu',
}

export const severityLabels: Record<DamageSeverity, string> = {
  Light: 'Hafif',
  Medium: 'Orta',
  Heavy: 'Ağır',
}

/** Rapora ait tüm sorguların ortak öneki; invalidate edilince detay, teklifler, seçim ve iletişim birlikte yenilenir. */
export const reportKeys = {
  all: (reportId: string) => ['reports', reportId] as const,
  detail: (reportId: string) => ['reports', reportId, 'detail'] as const,
}

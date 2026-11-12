// Rapor tipleri: oluşturma ve liste (UC-02, endpoint 14, 16, 17), detay (UC-04, endpoint 18). JSON camelCase, enum'lar metin.

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

/** POST /api/photos/temporary yanıtı; tempKey rapor oluştururken geri gönderilir. */
export interface TemporaryPhoto {
  tempKey: string
  width: number
  height: number
  sizeBytes: number
}

export interface ReportDamageInput {
  categoryId: number
  severity: DamageSeverity
  note?: string | null
}

/** POST /api/reports gövdesi. Koordinat cihazdan alınmadıysa ilin merkezi gönderilir ve ilçe zorunludur. */
export interface CreateReportRequest {
  vehicleTypeId: number
  description: string | null
  city: string
  district: string | null
  addressText: string | null
  latitude: number
  longitude: number
  isDeviceLocation: boolean
  damages: ReportDamageInput[]
  photoKeys: string[]
}

export interface CreateReportResponse {
  id: string
  status: ReportStatus
  expiresAt: string
}

/** "Raporlarım" satırı (endpoint 17). */
export interface ReportListItem {
  id: string
  status: ReportStatus
  city: string
  district: string | null
  vehicleTypeName: string
  offerCount: number
  lowestOfferPrice: number | null
  createdAt: string
  expiresAt: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
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
  /** Müşterinin rapor listesi; rapor oluşturulunca ve durum değişince yenilenir. */
  mine: ['reports', 'mine'] as const,
  minePage: (page: number) => ['reports', 'mine', page] as const,
}

export const photoKeys = {
  blob: (photoId: string) => ['photos', photoId] as const,
}

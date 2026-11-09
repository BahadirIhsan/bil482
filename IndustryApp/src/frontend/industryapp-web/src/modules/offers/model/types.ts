// UC-04 · Müşterinin gördüğü teklif listesi (GET /api/reports/{id}/offers?sort=, endpoint 27).

export type OfferStatus = 'Pending' | 'Accepted' | 'NotSelected' | 'Withdrawn' | 'Closed'

export type PriceItemType = 'Labor' | 'Parts' | 'Paint' | 'Warranty' | 'Other'

/** Sıralama anahtarları backend'deki IOfferSortStrategy.Key değerleriyle aynıdır (PAT-04.4). */
export type OfferSortKey = 'price' | 'fastest' | 'newest'

export interface OfferPriceLine {
  itemType: PriceItemType
  description: string | null
  amount: number
}

export interface OfferListItem {
  id: string
  mechanicId: string
  workshopName: string
  totalPrice: number
  currency: string
  estimatedDays: number
  note: string | null
  status: OfferStatus
  createdAt: string
  updatedAt: string | null
  priceItems: OfferPriceLine[]
}

export const offerStatusLabels: Record<OfferStatus, string> = {
  Pending: 'Değerlendirmede',
  Accepted: 'Seçildi',
  NotSelected: 'Seçilmedi',
  Withdrawn: 'Geri çekildi',
  Closed: 'Kapandı',
}

export const priceItemLabels: Record<PriceItemType, string> = {
  Labor: 'İşçilik',
  Parts: 'Parça',
  Paint: 'Boya',
  Warranty: 'Garanti',
  Other: 'Diğer',
}

export const offerSortLabels: Record<OfferSortKey, string> = {
  price: 'En düşük fiyat',
  fastest: 'En kısa süre',
  newest: 'En yeni',
}

export const offerKeys = {
  all: (reportId: string) => ['reports', reportId, 'offers'] as const,
  list: (reportId: string, sort: OfferSortKey) => ['reports', reportId, 'offers', sort] as const,
}

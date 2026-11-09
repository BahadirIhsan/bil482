// UC-04 · Eşleşme iletişim bilgisi (GET /api/reports/{id}/contact, endpoint 32).
// Değerler sunucudaki ContactInfoProxy'den gelir (PAT-04.3): isRevealed=false ise telefon maskeli, e-posta '-'.

export type ContactCounterpartRole = 'Customer' | 'Mechanic'

export interface MatchContact {
  reportId: string
  counterpartRole: ContactCounterpartRole
  fullName: string
  phone: string
  email: string
  isRevealed: boolean
  workshopName: string | null
  address: string | null
}

export const counterpartRoleLabels: Record<ContactCounterpartRole, string> = {
  Customer: 'Müşteri',
  Mechanic: 'Usta',
}

export const contactKeys = {
  match: (reportId: string) => ['reports', reportId, 'contact'] as const,
}

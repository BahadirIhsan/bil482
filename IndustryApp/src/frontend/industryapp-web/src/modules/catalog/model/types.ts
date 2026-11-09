// Katalog (GET /api/catalog/damage-categories, /legal-documents — endpoint 11, 13).

export interface DamageCategoryNode {
  id: number
  code: string
  name: string
  level: number
  children: DamageCategoryNode[]
}

export interface LegalDocument {
  id: number
  code: string
  version: string
  title: string
  body: string
}

export type ConsentRole = 'Customer' | 'Mechanic'

export const catalogKeys = {
  damageCategories: ['catalog', 'damage-categories'] as const,
  legalDocuments: (role: ConsentRole) => ['catalog', 'legal-documents', role] as const,
}

import { client } from '../../../shared/http/axiosClient'
import type { ConsentRole, DamageCategoryNode, LegalDocument } from './types'

export const catalogApi = {
  damageCategories: async (): Promise<DamageCategoryNode[]> =>
    (await client.get<DamageCategoryNode[]>('/catalog/damage-categories')).data,

  legalDocuments: async (role: ConsentRole): Promise<LegalDocument[]> =>
    (await client.get<LegalDocument[]>('/catalog/legal-documents', { params: { role } })).data,
}

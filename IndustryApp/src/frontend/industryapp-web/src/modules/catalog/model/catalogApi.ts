import { client } from '../../../shared/http/axiosClient'
import type { ConsentRole, DamageCategoryNode, LegalDocument, VehicleType } from './types'

export const catalogApi = {
  damageCategories: async (): Promise<DamageCategoryNode[]> =>
    (await client.get<DamageCategoryNode[]>('/catalog/damage-categories')).data,

  vehicleTypes: async (): Promise<VehicleType[]> =>
    (await client.get<VehicleType[]>('/catalog/vehicle-types')).data,

  legalDocuments: async (role: ConsentRole): Promise<LegalDocument[]> =>
    (await client.get<LegalDocument[]>('/catalog/legal-documents', { params: { role } })).data,
}

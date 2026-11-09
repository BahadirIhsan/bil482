import { client } from '../../../shared/http/axiosClient'
import type { MatchContact } from './types'

export const contactsApi = {
  get: async (reportId: string): Promise<MatchContact> =>
    (await client.get<MatchContact>(`/reports/${reportId}/contact`)).data,
}

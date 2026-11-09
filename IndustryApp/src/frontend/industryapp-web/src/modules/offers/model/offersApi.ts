import { client } from '../../../shared/http/axiosClient'
import type { OfferListItem, OfferSortKey } from './types'

// UC-04 · Rapordaki teklifler; sıralama sunucuda (SQL ORDER BY) yapılır.
export const offersApi = {
  listForReport: async (reportId: string, sort: OfferSortKey): Promise<OfferListItem[]> =>
    (await client.get<OfferListItem[]>(`/reports/${reportId}/offers`, { params: { sort } })).data,
}

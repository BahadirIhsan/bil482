import { client } from '../../../shared/http/axiosClient'
import type { SelectionResponse, SelectionStatus } from './types'

// UC-04 · Seçim, geri alma ve seçim durumu (endpoint 29, 30, 31).
export const selectionApi = {
  select: async (reportId: string, offerId: string): Promise<SelectionResponse> =>
    (await client.post<SelectionResponse>(`/reports/${reportId}/selection`, { offerId })).data,

  undo: async (reportId: string): Promise<SelectionStatus> =>
    (await client.delete<SelectionStatus>(`/reports/${reportId}/selection`)).data,

  get: async (reportId: string): Promise<SelectionStatus> =>
    (await client.get<SelectionStatus>(`/reports/${reportId}/selection`)).data,
}

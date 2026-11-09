import { client } from '../../../shared/http/axiosClient'
import type { ReportDetail } from './types'

// UC-04 · Müşterinin rapor uç noktaları (endpoint 18, 19, 20).
export const reportsApi = {
  getForCustomer: async (reportId: string): Promise<ReportDetail> =>
    (await client.get<ReportDetail>(`/reports/${reportId}`)).data,

  complete: async (reportId: string): Promise<void> => {
    await client.post(`/reports/${reportId}/complete`)
  },

  cancel: async (reportId: string): Promise<void> => {
    await client.post(`/reports/${reportId}/cancel`)
  },
}

import { client } from '../../../shared/http/axiosClient'
import type { CreateReportRequest, CreateReportResponse, PagedResult, ReportDetail, ReportListItem } from './types'

// Müşterinin rapor uç noktaları: oluşturma ve liste (UC-02, endpoint 16, 17), detay, tamamlama, iptal (UC-04, 18–20).
export const reportsApi = {
  create: async (request: CreateReportRequest): Promise<CreateReportResponse> =>
    (await client.post<CreateReportResponse>('/reports', request)).data,

  listMine: async (page: number, pageSize = 10): Promise<PagedResult<ReportListItem>> =>
    (await client.get<PagedResult<ReportListItem>>('/reports', { params: { page, pageSize } })).data,

  getForCustomer: async (reportId: string): Promise<ReportDetail> =>
    (await client.get<ReportDetail>(`/reports/${reportId}`)).data,

  complete: async (reportId: string): Promise<void> => {
    await client.post(`/reports/${reportId}/complete`)
  },

  cancel: async (reportId: string): Promise<void> => {
    await client.post(`/reports/${reportId}/cancel`)
  },
}

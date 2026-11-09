import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useRealtimeEvent } from '../../../shared/realtime'
import type { ReportStatusChangedPayload } from '../../selection/model/types'
import { reportsApi } from '../model/reportsApi'
import { reportKeys } from '../model/types'

/**
 * UC-04 · Rapor detayı. Sunucu report.statusChanged yayınlayınca (seçim, geri alma, tamamlama, iptal)
 * raporun tüm sorguları yenilenir; asıl kaynak her zaman REST'tir (T-12).
 */
export function useCustomerReport(reportId: string) {
  const queryClient = useQueryClient()

  useRealtimeEvent<ReportStatusChangedPayload>('report.statusChanged', (payload) => {
    if (payload.reportId === reportId) void queryClient.invalidateQueries({ queryKey: reportKeys.all(reportId) })
  })

  return useQuery({
    queryKey: reportKeys.detail(reportId),
    queryFn: () => reportsApi.getForCustomer(reportId),
    enabled: reportId.length > 0,
  })
}

/** FR-04.5 · İşi tamamlandı olarak işaretler. */
export function useCompleteReport(reportId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => reportsApi.complete(reportId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: reportKeys.all(reportId) }),
  })
}

/** FR-04.5 · Raporu iptal eder; bekleyen teklifler kapanır. */
export function useCancelReport(reportId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => reportsApi.cancel(reportId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: reportKeys.all(reportId) }),
  })
}

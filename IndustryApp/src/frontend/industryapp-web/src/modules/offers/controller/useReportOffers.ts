import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useRealtimeEvent } from '../../../shared/realtime'
import { offersApi } from '../model/offersApi'
import { offerKeys, type OfferSortKey } from '../model/types'

interface OfferEventPayload {
  reportId: string
}

/**
 * UC-04 · FR-04.1 · Teklifleri seçilen sıralamayla getirir ve sayfa yenilemeden günceller:
 * offer.created / offer.updated / offer.withdrawn gelince bu raporun teklif sorguları yenilenir.
 */
export function useReportOffers(reportId: string, sort: OfferSortKey) {
  const queryClient = useQueryClient()

  const refresh = (payload: OfferEventPayload) => {
    if (payload.reportId === reportId) void queryClient.invalidateQueries({ queryKey: offerKeys.all(reportId) })
  }
  useRealtimeEvent<OfferEventPayload>('offer.created', refresh)
  useRealtimeEvent<OfferEventPayload>('offer.updated', refresh)
  useRealtimeEvent<OfferEventPayload>('offer.withdrawn', refresh)

  return useQuery({
    queryKey: offerKeys.list(reportId, sort),
    queryFn: () => offersApi.listForReport(reportId, sort),
    enabled: reportId.length > 0,
  })
}

import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query'
import { useRealtimeEvent } from '../../../shared/realtime'
import { reportsApi } from '../model/reportsApi'
import { reportKeys } from '../model/types'

/** UC-02 · "Raporlarım". Yeni teklif veya durum değişikliği gelince liste sayfa yenilenmeden güncellenir. */
export function useMyReports(page: number) {
  const queryClient = useQueryClient()
  const refresh = () => void queryClient.invalidateQueries({ queryKey: reportKeys.mine })

  useRealtimeEvent('report.statusChanged', refresh)
  useRealtimeEvent('offer.created', refresh)
  useRealtimeEvent('offer.withdrawn', refresh)

  return useQuery({
    queryKey: reportKeys.minePage(page),
    queryFn: () => reportsApi.listMine(page),
    placeholderData: keepPreviousData,
  })
}

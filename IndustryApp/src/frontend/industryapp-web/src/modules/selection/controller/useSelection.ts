import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { reportKeys } from '../../reports/model/types'
import { selectionApi } from '../model/selectionApi'
import { selectionKeys } from '../model/types'

/** UC-04 · Raporun seçim durumu: seçilen teklif, geri alma son anı, geri alınabilir mi. */
export function useSelectionStatus(reportId: string) {
  return useQuery({
    queryKey: selectionKeys.status(reportId),
    queryFn: () => selectionApi.get(reportId),
    enabled: reportId.length > 0,
  })
}

/** FR-04.2 · Teklif seçer. Başarıda rapor, teklif ve seçim sorguları birlikte yenilenir. */
export function useSelectOffer(reportId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (offerId: string) => selectionApi.select(reportId, offerId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: reportKeys.all(reportId) }),
  })
}

/** FR-04.3 · Seçimi süre içinde geri alır; süre dışında sunucu 409 UNDO_WINDOW_EXPIRED döner. */
export function useUndoSelection(reportId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => selectionApi.undo(reportId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: reportKeys.all(reportId) }),
  })
}

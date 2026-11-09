import { useQuery } from '@tanstack/react-query'
import { contactsApi } from '../model/contactsApi'
import { contactKeys } from '../model/types'

/** UC-04 · FR-04.4 · Karşı tarafın iletişim bilgisi. 403 (eşleşme yok) tekrar denenmez. */
export function useMatchContact(reportId: string) {
  return useQuery({
    queryKey: contactKeys.match(reportId),
    queryFn: () => contactsApi.get(reportId),
    enabled: reportId.length > 0,
    retry: false,
  })
}

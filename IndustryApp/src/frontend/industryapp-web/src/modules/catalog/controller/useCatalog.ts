import { useQuery } from '@tanstack/react-query'
import { catalogApi } from '../model/catalogApi'
import { catalogKeys, type ConsentRole } from '../model/types'

/** Hasar ağacı; usta kaydında yalnızca 1. seviye (kök) düğümler uzmanlık olarak gösterilir. */
export function useDamageCategories() {
  return useQuery({
    queryKey: catalogKeys.damageCategories,
    queryFn: catalogApi.damageCategories,
    staleTime: 10 * 60_000,
  })
}

/** Rolün kayıtta onaylaması gereken güncel sözleşmeler. */
export function useLegalDocuments(role: ConsentRole) {
  return useQuery({
    queryKey: catalogKeys.legalDocuments(role),
    queryFn: () => catalogApi.legalDocuments(role),
    staleTime: 10 * 60_000,
  })
}

import { useQuery } from '@tanstack/react-query'
import { catalogApi } from '../model/catalogApi'
import { catalogKeys, type ConsentRole } from '../model/types'

/** Katalog sunucuda da 10 dakika önbellektedir; istemci aynı süre boyunca yeniden istemez (NFR-P1). */
const CATALOG_STALE_TIME = 10 * 60_000

/**
 * Hasar ağacı (Composite, PAT-02.2). Usta kaydında yalnızca 1. seviye düğümler uzmanlık olarak, rapor
 * sihirbazında bütün ağaç gösterilir.
 */
export function useDamageCategories() {
  return useQuery({
    queryKey: catalogKeys.damageCategories,
    queryFn: catalogApi.damageCategories,
    staleTime: CATALOG_STALE_TIME,
  })
}

/** Rapor sihirbazının adıyla aynı hook (SDD 6.1). */
export const useDamageTree = useDamageCategories

/** UC-02 · Rapor sihirbazının araç türü seçenekleri. */
export function useVehicleTypes() {
  return useQuery({
    queryKey: catalogKeys.vehicleTypes,
    queryFn: catalogApi.vehicleTypes,
    staleTime: CATALOG_STALE_TIME,
  })
}

/** Rolün kayıtta onaylaması gereken güncel sözleşmeler. */
export function useLegalDocuments(role: ConsentRole) {
  return useQuery({
    queryKey: catalogKeys.legalDocuments(role),
    queryFn: () => catalogApi.legalDocuments(role),
    staleTime: CATALOG_STALE_TIME,
  })
}

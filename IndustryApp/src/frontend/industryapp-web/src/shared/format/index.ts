export { formatDate } from './formatDate'

/** Enum/kod değerini Türkçe etikete çevirir; eşleşme yoksa veya değer boşsa '-' (ham kod ekrana çıkmaz). */
export const labelOf = <T extends string>(map: Record<T, string>, value?: T | string | null): string =>
  value && value in map ? map[value as T] : '-'

/** 12500 → "₺12.500,00" (tr-TR). Boş değer '-'. */
export const formatMoney = (amount?: number | null, currency = 'TRY'): string =>
  amount == null ? '-' : new Intl.NumberFormat('tr-TR', { style: 'currency', currency }).format(amount)

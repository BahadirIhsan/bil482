import type { AuthUser } from '../../../shared/auth'
import type { NotificationItem } from './types'

const matchTypes = new Set(['OFFER_SELECTED', 'JOB_COMPLETED'])

/**
 * Bildirime tıklanınca gidilecek ekran: payload.reportId ile. Müşteri rapor detayına, usta seçildiği/tamamlanan
 * işte eşleşme ekranına, diğer durumlarda iş detayına gider. Bağlantı yoksa null.
 */
export function notificationLink(notification: Pick<NotificationItem, 'type' | 'payload'>, user: AuthUser | null): string | null {
  let reportId: string | undefined
  try {
    reportId = notification.payload ? (JSON.parse(notification.payload) as { reportId?: string }).reportId : undefined
  } catch {
    reportId = undefined
  }
  if (!reportId || !user) return null

  if (user.roles.includes('Customer')) return `/customer/reports/${reportId}`
  if (user.roles.includes('Mechanic')) return matchTypes.has(notification.type) ? `/match/${reportId}` : `/mechanic/jobs/${reportId}`
  return null
}

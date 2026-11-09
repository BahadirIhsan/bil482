// Bildirimler (GET/POST /api/notifications… — endpoint 33–36; SignalR notification.created).

export interface NotificationItem {
  id: string
  type: string
  title: string
  body: string
  /** JSON metni; ör. {"reportId":"…"} → tıklanınca ilgili ekrana gidilir. */
  payload: string | null
  isRead: boolean
  createdAt: string
  readAt: string | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

/** notification.created yükü. */
export type NotificationCreated = Omit<NotificationItem, 'isRead' | 'readAt'>

export const notificationTypeLabels: Record<string, string> = {
  NEW_JOB: 'Yeni iş',
  NO_MECHANIC_FOUND: 'Usta bulunamadı',
  NEW_OFFER: 'Yeni teklif',
  OFFER_SELECTED: 'Teklifiniz seçildi',
  OFFER_NOT_SELECTED: 'Teklif seçilmedi',
  SELECTION_UNDONE: 'Seçim geri alındı',
  JOB_COMPLETED: 'İş tamamlandı',
  REPORT_CANCELLED: 'Rapor iptal edildi',
  REPORT_EXPIRED: 'Rapor süresi doldu',
  MECHANIC_APPROVED: 'Hesap onaylandı',
  MECHANIC_REJECTED: 'Hesap reddedildi',
}

export const notificationKeys = {
  all: ['notifications'] as const,
  unreadCount: ['notifications', 'unread-count'] as const,
  list: (page: number, pageSize: number) => ['notifications', 'list', page, pageSize] as const,
}

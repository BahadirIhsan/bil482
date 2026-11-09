import { client } from '../../../shared/http/axiosClient'
import type { NotificationItem, PagedResult } from './types'

export const notificationsApi = {
  list: async (page: number, pageSize: number): Promise<PagedResult<NotificationItem>> =>
    (await client.get<PagedResult<NotificationItem>>('/notifications', { params: { page, pageSize } })).data,

  unreadCount: async (): Promise<number> => (await client.get<{ count: number }>('/notifications/unread-count')).data.count,

  markRead: async (id: string): Promise<void> => {
    await client.post(`/notifications/${id}/read`)
  },

  markAllRead: async (): Promise<void> => {
    await client.post('/notifications/read-all')
  },
}

import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '../../../shared/auth'
import { useRealtimeEvent } from '../../../shared/realtime'
import { notificationsApi } from '../model/notificationsApi'
import { notificationKeys, type NotificationCreated } from '../model/types'

/**
 * Bildirim zili için: okunmamış sayısı, son 10 bildirim ve okundu işlemleri. notification.created gelince
 * sayaç hemen artar ve liste yenilenir; latest son gelen bildirimi pop-up için tutar.
 */
export function useNotifications() {
  const { status } = useAuth()
  const queryClient = useQueryClient()
  const enabled = status === 'authenticated'
  const [latest, setLatest] = useState<NotificationCreated | null>(null)

  useRealtimeEvent<NotificationCreated>('notification.created', (payload) => {
    queryClient.setQueryData<number>(notificationKeys.unreadCount, (count) => (count ?? 0) + 1)
    void queryClient.invalidateQueries({ queryKey: notificationKeys.all, refetchType: 'active' })
    setLatest(payload)
  })

  const unreadCount = useQuery({ queryKey: notificationKeys.unreadCount, queryFn: notificationsApi.unreadCount, enabled })
  const recent = useQuery({ queryKey: notificationKeys.list(1, 10), queryFn: () => notificationsApi.list(1, 10), enabled })

  return {
    unreadCount: unreadCount.data ?? 0,
    recent,
    latest,
    dismissLatest: () => setLatest(null),
    markRead: useMarkRead(),
    markAllRead: useMarkAllRead(),
  }
}

/** Bildirimler sayfası (view #6): sayfalı liste. */
export function useNotificationPage(page: number, pageSize = 20) {
  return useQuery({
    queryKey: notificationKeys.list(page, pageSize),
    queryFn: () => notificationsApi.list(page, pageSize),
    placeholderData: (previous) => previous,
  })
}

export function useMarkRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: notificationsApi.markRead,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationKeys.all }),
  })
}

export function useMarkAllRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: notificationsApi.markAllRead,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationKeys.all }),
  })
}

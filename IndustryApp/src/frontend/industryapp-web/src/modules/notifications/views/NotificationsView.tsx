import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Alert,
  Button,
  Chip,
  List,
  ListItemButton,
  ListItemText,
  Pagination,
  Paper,
  Stack,
  Typography,
} from '@mui/material'
import { useAuth } from '../../../shared/auth'
import { formatDate, labelOf } from '../../../shared/format'
import { PageLoader } from '../../../shared/ui/PageLoader'
import { errorText } from '../../selection/model/errorText'
import { useMarkAllRead, useMarkRead, useNotificationPage } from '../controller/useNotifications'
import { notificationLink } from '../model/notificationLink'
import { notificationTypeLabels, type NotificationItem } from '../model/types'

const PAGE_SIZE = 20

/** Bildirimler sayfası (route /notifications, view #6): tüm bildirimler, sayfalı. */
export function NotificationsView() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const notifications = useNotificationPage(page, PAGE_SIZE)
  const markRead = useMarkRead()
  const markAllRead = useMarkAllRead()

  const open = (n: NotificationItem) => {
    if (!n.isRead) markRead.mutate(n.id)
    const link = notificationLink(n, user)
    if (link) navigate(link)
  }

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h5" component="h1">
          Bildirimler
        </Typography>
        <Button disabled={markAllRead.isPending} onClick={() => markAllRead.mutate()}>
          Tümünü okundu yap
        </Button>
      </Stack>

      {notifications.isPending ? (
        <PageLoader />
      ) : notifications.isError ? (
        <Alert severity="error">{errorText(notifications.error, 'Bildirimler yüklenemedi.')}</Alert>
      ) : notifications.data.items.length === 0 ? (
        <Alert severity="info">Henüz bildiriminiz yok.</Alert>
      ) : (
        <Paper variant="outlined">
          <List disablePadding>
            {notifications.data.items.map((n) => (
              <ListItemButton key={n.id} divider onClick={() => open(n)} sx={{ bgcolor: n.isRead ? undefined : 'action.hover' }}>
                <ListItemText
                  primary={
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography fontWeight={n.isRead ? 400 : 600}>{n.title || '-'}</Typography>
                      <Chip size="small" variant="outlined" label={labelOf(notificationTypeLabels, n.type)} />
                    </Stack>
                  }
                  secondary={`${n.body || '-'} · ${formatDate(n.createdAt, true)}`}
                />
              </ListItemButton>
            ))}
          </List>
        </Paper>
      )}

      {notifications.data && notifications.data.totalPages > 1 && (
        <Pagination
          count={notifications.data.totalPages}
          page={page}
          onChange={(_, value) => setPage(value)}
          sx={{ alignSelf: 'center' }}
        />
      )}
    </Stack>
  )
}

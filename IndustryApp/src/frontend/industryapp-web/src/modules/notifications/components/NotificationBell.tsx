import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Alert,
  Badge,
  Box,
  Button,
  CircularProgress,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Popover,
  Snackbar,
  Stack,
  Typography,
} from '@mui/material'
import NotificationsIcon from '@mui/icons-material/Notifications'
import { useAuth } from '../../../shared/auth'
import { formatDate } from '../../../shared/format'
import { useNotifications } from '../controller/useNotifications'
import { notificationLink } from '../model/notificationLink'
import type { NotificationItem } from '../model/types'

/**
 * Layout'taki bildirim zili (T-14.4): okunmamış sayısı notification.created ile artar, tıklanınca son 10
 * bildirim açılır; bildirime tıklamak okundu yapar ve payload.reportId ile ilgili ekrana gider.
 */
export function NotificationBell() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const { unreadCount, recent, latest, dismissLatest, markRead, markAllRead } = useNotifications()
  const [anchor, setAnchor] = useState<HTMLElement | null>(null)

  const open = (notification: Pick<NotificationItem, 'id' | 'type' | 'payload'> & { isRead?: boolean }) => {
    if (!notification.isRead) markRead.mutate(notification.id)
    setAnchor(null)
    const link = notificationLink(notification, user)
    navigate(link ?? '/notifications')
  }

  const items = recent.data?.items ?? []

  return (
    <>
      <IconButton
        color="inherit"
        aria-label={`Bildirimler (${unreadCount} okunmamış)`}
        onClick={(e) => setAnchor(e.currentTarget)}
      >
        <Badge badgeContent={unreadCount} color="error" max={99}>
          <NotificationsIcon />
        </Badge>
      </IconButton>

      <Popover
        open={anchor !== null}
        anchorEl={anchor}
        onClose={() => setAnchor(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        slotProps={{ paper: { sx: { width: 360, maxWidth: 'calc(100vw - 32px)' } } }}
      >
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ px: 2, py: 1 }}>
          <Typography variant="subtitle1">Bildirimler</Typography>
          <Button size="small" disabled={unreadCount === 0 || markAllRead.isPending} onClick={() => markAllRead.mutate()}>
            Tümünü okundu yap
          </Button>
        </Stack>
        <Divider />
        {recent.isPending ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}>
            <CircularProgress size={24} aria-label="Bildirimler yükleniyor" />
          </Box>
        ) : items.length === 0 ? (
          <Typography color="text.secondary" sx={{ p: 2 }}>
            Bildiriminiz yok.
          </Typography>
        ) : (
          <List dense disablePadding sx={{ maxHeight: 400, overflowY: 'auto' }}>
            {items.map((n) => (
              <ListItemButton key={n.id} onClick={() => open(n)} sx={{ bgcolor: n.isRead ? undefined : 'action.hover' }}>
                <ListItemText
                  primary={n.title || '-'}
                  primaryTypographyProps={{ fontWeight: n.isRead ? 400 : 600 }}
                  secondary={`${n.body || '-'} · ${formatDate(n.createdAt, true)}`}
                />
              </ListItemButton>
            ))}
          </List>
        )}
        <Divider />
        <Box sx={{ p: 1, textAlign: 'center' }}>
          <Button
            size="small"
            onClick={() => {
              setAnchor(null)
              navigate('/notifications')
            }}
          >
            Tümünü gör
          </Button>
        </Box>
      </Popover>

      <Snackbar
        open={latest !== null}
        autoHideDuration={6000}
        onClose={dismissLatest}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          severity="info"
          variant="filled"
          onClose={dismissLatest}
          action={
            latest && (
              <Button color="inherit" size="small" onClick={() => { open(latest); dismissLatest() }}>
                Aç
              </Button>
            )
          }
        >
          <strong>{latest?.title}</strong> {latest?.body}
        </Alert>
      </Snackbar>
    </>
  )
}

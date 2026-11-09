import { useState, type ReactNode } from 'react'
import { Link as RouterLink, NavLink, Outlet, useNavigate } from 'react-router-dom'
import {
  AppBar,
  Box,
  Button,
  Container,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Menu,
  MenuItem,
  Stack,
  Toolbar,
  Typography,
} from '@mui/material'
import AccountCircleIcon from '@mui/icons-material/AccountCircle'
import MenuIcon from '@mui/icons-material/Menu'
import { useAuth } from '../../shared/auth'
import { NotificationBell } from '../../modules/notifications/components/NotificationBell'

export interface MenuLink {
  to: string
  label: string
  /** true ise yalnızca tam yol eşleşmesinde aktif görünür. */
  end?: boolean
}

interface AppShellProps {
  /** Başlıktaki rol adı: "IndustryApp · Müşteri". */
  area: string
  menu: MenuLink[]
  children?: ReactNode
}

/**
 * Giriş yapmış kullanıcı layout'larının ortak iskeleti: üst çubuk, rol menüsü, bildirim zili ve hesap menüsü.
 * Menüler bu sürümde layout'ta sabittir; v0.4'te /api/account/dashboard'dan (Abstract Factory) gelir.
 */
export function AppShell({ area, menu, children }: AppShellProps) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [accountAnchor, setAccountAnchor] = useState<HTMLElement | null>(null)

  const signOut = async () => {
    setAccountAnchor(null)
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="sticky" elevation={0}>
        <Toolbar sx={{ gap: 1 }}>
          <IconButton color="inherit" edge="start" aria-label="Menüyü aç" onClick={() => setDrawerOpen(true)}
            sx={{ display: { md: 'none' } }}>
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" component={RouterLink} to="/" sx={{ color: 'inherit', textDecoration: 'none', mr: 2 }} noWrap>
            IndustryApp · {area}
          </Typography>
          <Stack direction="row" spacing={0.5} sx={{ display: { xs: 'none', md: 'flex' }, flexGrow: 1 }}>
            {menu.map((item) => (
              <Button key={item.to} component={NavLink} to={item.to} end={item.end} color="inherit"
                sx={{ '&.active': { bgcolor: 'rgba(255,255,255,0.16)' } }}>
                {item.label}
              </Button>
            ))}
          </Stack>
          <Box sx={{ flexGrow: { xs: 1, md: 0 } }} />
          <NotificationBell />
          <IconButton color="inherit" aria-label="Hesap menüsü" onClick={(e) => setAccountAnchor(e.currentTarget)}>
            <AccountCircleIcon />
          </IconButton>
          <Menu anchorEl={accountAnchor} open={accountAnchor !== null} onClose={() => setAccountAnchor(null)}>
            <MenuItem disabled>
              <Typography variant="body2">{user?.fullName || '-'}</Typography>
            </MenuItem>
            <Divider />
            <MenuItem component={RouterLink} to="/profile" onClick={() => setAccountAnchor(null)}>
              Profil
            </MenuItem>
            <MenuItem component={RouterLink} to="/notifications" onClick={() => setAccountAnchor(null)}>
              Bildirimler
            </MenuItem>
            <MenuItem onClick={() => void signOut()}>Çıkış yap</MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      <Drawer open={drawerOpen} onClose={() => setDrawerOpen(false)}>
        <List sx={{ width: 260 }}>
          {menu.map((item) => (
            <ListItemButton key={item.to} component={NavLink} to={item.to} end={item.end} onClick={() => setDrawerOpen(false)}>
              <ListItemText primary={item.label} />
            </ListItemButton>
          ))}
        </List>
      </Drawer>

      <Container maxWidth="lg" sx={{ py: { xs: 2, sm: 4 } }}>{children ?? <Outlet />}</Container>
    </Box>
  )
}

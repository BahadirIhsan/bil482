import { primaryRole, useAuth } from '../../shared/auth'
import { AppShell, type MenuLink } from './AppShell'

// Rol menüleri bu sürümde sabittir (rehber 2.4); v0.4'te backend'deki Abstract Factory'den (PAT-01.2) gelir.
const customerMenu: MenuLink[] = [
  { to: '/customer', label: 'Panelim', end: true },
  { to: '/customer/reports', label: 'Raporlarım', end: true },
  { to: '/customer/reports/new', label: 'Yeni rapor' },
]

const mechanicMenu: MenuLink[] = [
  { to: '/mechanic', label: 'Panelim', end: true },
  { to: '/mechanic/jobs', label: 'İşler' },
  { to: '/mechanic/offers', label: 'Tekliflerim' },
]

const pendingMechanicMenu: MenuLink[] = [{ to: '/mechanic/pending', label: 'Hesap durumu' }]

const adminMenu: MenuLink[] = [
  { to: '/admin', label: 'Panel', end: true },
  { to: '/admin/mechanics', label: 'Usta onayları' },
  { to: '/admin/categories', label: 'Kategoriler' },
]

export const CustomerLayout = () => <AppShell area="Müşteri" menu={customerMenu} />

export function MechanicLayout() {
  const { user } = useAuth()
  return <AppShell area="Usta" menu={user?.mechanicStatus === 'Approved' ? mechanicMenu : pendingMechanicMenu} />
}

export const AdminLayout = () => <AppShell area="Yönetim" menu={adminMenu} />

/** Rol bağımsız sayfalar (profil, bildirimler, eşleşme) kullanıcının kendi rol layout'unda açılır. */
export function RoleLayout() {
  const { user } = useAuth()
  switch (primaryRole(user)) {
    case 'Admin':
      return <AdminLayout />
    case 'Mechanic':
      return <MechanicLayout />
    default:
      return <CustomerLayout />
  }
}

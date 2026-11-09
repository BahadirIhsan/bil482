import { createBrowserRouter } from 'react-router-dom'
import { RequireRole } from '../shared/auth'
import { ComingSoon } from '../shared/ui/ComingSoon'
import { CustomerDashboardView } from '../modules/account/views/CustomerDashboardView'
import { MechanicPendingApprovalView } from '../modules/account/views/MechanicPendingApprovalView'
import { ProfileView } from '../modules/account/views/ProfileView'
import { LoginView } from '../modules/auth/views/LoginView'
import { RegisterCustomerView } from '../modules/auth/views/RegisterCustomerView'
import { RegisterMechanicView } from '../modules/auth/views/RegisterMechanicView'
import { RegisterRoleSelectView } from '../modules/auth/views/RegisterRoleSelectView'
import { MatchView } from '../modules/contacts/views/MatchView'
import { NotificationsView } from '../modules/notifications/views/NotificationsView'
import { CustomerReportDetailView } from '../modules/reports/views/CustomerReportDetailView'
import { ErrorView } from '../modules/system/views/ErrorView'
import { AdminLayout, CustomerLayout, MechanicLayout, RoleLayout } from './layouts/roleLayouts'
import { PublicLayout } from './layouts/PublicLayout'
import { HomeRedirect } from './HomeRedirect'

/**
 * T-14.3 route'ları. v0.2'de 1–8 ve 12 numaralı view'lar ile rol yönlendirmesi aktiftir; diğerleri
 * hangi sürümde geleceğini söyleyen yer tutucudur.
 */
export const router = createBrowserRouter([
  { path: '/', element: <HomeRedirect /> },
  {
    element: <PublicLayout />,
    children: [
      { path: '/login', element: <LoginView /> },
      { path: '/register', element: <RegisterRoleSelectView /> },
      { path: '/register/customer', element: <RegisterCustomerView /> },
      { path: '/register/mechanic', element: <RegisterMechanicView /> },
    ],
  },
  {
    element: (
      <RequireRole>
        <RoleLayout />
      </RequireRole>
    ),
    children: [
      { path: '/profile', element: <ProfileView /> },
      { path: '/notifications', element: <NotificationsView /> },
      { path: '/match/:reportId', element: <MatchView /> },
      { path: '/403', element: <ErrorView code={403} /> },
    ],
  },
  {
    element: (
      <RequireRole roles={['Customer']}>
        <CustomerLayout />
      </RequireRole>
    ),
    children: [
      { path: '/customer', element: <CustomerDashboardView /> },
      { path: '/customer/reports', element: <ComingSoon title="Raporlarım" release="v0.3" /> },
      { path: '/customer/reports/new', element: <ComingSoon title="Yeni kaza raporu" release="v0.3" /> },
      { path: '/customer/reports/:reportId', element: <CustomerReportDetailView /> },
    ],
  },
  {
    element: (
      <RequireRole roles={['Mechanic']}>
        <MechanicLayout />
      </RequireRole>
    ),
    children: [{ path: '/mechanic/pending', element: <MechanicPendingApprovalView /> }],
  },
  {
    element: (
      <RequireRole roles={['Mechanic']} requireApprovedMechanic>
        <MechanicLayout />
      </RequireRole>
    ),
    children: [
      { path: '/mechanic', element: <ComingSoon title="Usta paneli" release="v0.4" /> },
      { path: '/mechanic/jobs', element: <ComingSoon title="İşler" release="v0.3" /> },
      { path: '/mechanic/jobs/:reportId', element: <ComingSoon title="İş detayı" release="v0.3" /> },
      { path: '/mechanic/offers', element: <ComingSoon title="Tekliflerim" release="v0.3" /> },
    ],
  },
  {
    element: (
      <RequireRole roles={['Admin']}>
        <AdminLayout />
      </RequireRole>
    ),
    children: [
      { path: '/admin', element: <ComingSoon title="Yönetim paneli" release="v0.4" /> },
      { path: '/admin/mechanics', element: <ComingSoon title="Usta onayları" release="v0.3" /> },
      { path: '/admin/categories', element: <ComingSoon title="Kategori yönetimi" release="v0.3" /> },
    ],
  },
  { path: '*', element: <ErrorView code={404} /> },
])

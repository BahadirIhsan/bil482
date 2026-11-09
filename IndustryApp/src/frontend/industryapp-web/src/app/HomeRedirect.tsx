import { Navigate } from 'react-router-dom'
import { homePathFor, useAuth } from '../shared/auth'
import { PageLoader } from '../shared/ui/PageLoader'

/** "/" → oturum varsa rol paneline, yoksa girişe (T-14.3). */
export function HomeRedirect() {
  const { status, user } = useAuth()
  if (status === 'loading') return <PageLoader />
  return <Navigate to={status === 'authenticated' ? homePathFor(user) : '/login'} replace />
}

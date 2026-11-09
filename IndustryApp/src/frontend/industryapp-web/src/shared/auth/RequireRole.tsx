import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { PageLoader } from '../ui/PageLoader'
import { useAuth } from './AuthContext'
import type { Role } from './types'

interface RequireRoleProps {
  /** Boşsa yalnızca giriş yapmış olmak yeter. */
  roles?: Role[]
  /** Usta rotaları: onaylı değilse /mechanic/pending'e gider. */
  requireApprovedMechanic?: boolean
  children: ReactNode
}

/** Route koruması: giriş yoksa /login, rol uymuyorsa /403 (T-14.3). Asıl yetki kontrolü sunucudadır. */
export function RequireRole({ roles, requireApprovedMechanic = false, children }: RequireRoleProps) {
  const { status, user } = useAuth()
  const location = useLocation()

  if (status === 'loading') return <PageLoader />
  if (status === 'anonymous' || !user) return <Navigate to="/login" replace state={{ from: location }} />
  if (roles && !roles.some((role) => user.roles.includes(role))) return <Navigate to="/403" replace />
  if (requireApprovedMechanic && user.mechanicStatus !== 'Approved') return <Navigate to="/mechanic/pending" replace />

  return <>{children}</>
}

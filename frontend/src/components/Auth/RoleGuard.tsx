import { Navigate } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'

interface RoleGuardProps {
  children: React.ReactNode
  requiredRole: 'Admin' | 'SuperAdmin'
}

export function RoleGuard({ children, requiredRole }: RoleGuardProps) {
  const { user, isSuperAdmin, isAdmin } = useAuth()

  if (!user) return <Navigate to="/login" replace />

  if (requiredRole === 'SuperAdmin' && !isSuperAdmin) {
    return (
      <div className="d-flex justify-content-center align-items-center vh-100">
        <div className="text-center">
          <h2 className="text-danger">403 — Forbidden</h2>
          <p className="text-muted">You do not have permission to access this page.</p>
        </div>
      </div>
    )
  }

  if (requiredRole === 'Admin' && !isAdmin) {
    return (
      <div className="d-flex justify-content-center align-items-center vh-100">
        <div className="text-center">
          <h2 className="text-danger">403 — Forbidden</h2>
          <p className="text-muted">You do not have permission to access this page.</p>
        </div>
      </div>
    )
  }

  return <>{children}</>
}


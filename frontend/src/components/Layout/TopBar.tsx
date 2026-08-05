import { Button, Badge } from 'react-bootstrap'
import { useAuth } from '../../contexts/AuthContext'

interface TopBarProps {
  onToggleSidebar: () => void
}

export function TopBar({ onToggleSidebar }: TopBarProps) {
  const { user, logout } = useAuth()

  return (
    <header
      className="d-flex align-items-center justify-content-between px-3 px-md-4 py-2 border-bottom"
      style={{ backgroundColor: 'var(--act-page-bg, #fff)', color: 'var(--act-page-text, inherit)' }}
    >
      <Button
        variant="outline-dark"
        size="sm"
        className="d-lg-none"
        onClick={onToggleSidebar}
        aria-label="Toggle navigation menu"
      >
        ☰
      </Button>

      <div className="d-none d-lg-block">
        <h6 className="mb-0 text-muted">Aesthetic Client Tracker</h6>
      </div>

      <div className="d-flex align-items-center gap-2">
        <span className="d-none d-sm-inline text-muted small">{user?.email}</span>
        <Badge bg="info" className="text-capitalize">{user?.role}</Badge>
        <Button variant="outline-danger" size="sm" onClick={logout}>
          Logout
        </Button>
      </div>
    </header>
  )
}


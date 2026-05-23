import { NavLink } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'
import styles from './AppShell.module.css'

interface SidebarProps {
  isOpen: boolean
  onClose: () => void
}

export function Sidebar({ isOpen, onClose }: SidebarProps) {
  const { isSuperAdmin, isAdmin } = useAuth()

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `${styles.navLink} ${isActive ? styles.navLinkActive : ''}`

  return (
    <nav
      className={`${styles.sidebar} ${isOpen ? styles.sidebarOpen : ''} d-lg-block bg-dark text-white p-3`}
    >
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h5 className="mb-0 fw-bold text-white">ACT</h5>
        <button
          className="btn btn-sm btn-outline-light d-lg-none"
          onClick={onClose}
        >
          ✕
        </button>
      </div>

      <ul className="nav flex-column gap-1">
        <li><NavLink to="/" className={linkClass} onClick={onClose}>Dashboard</NavLink></li>
        <li><NavLink to="/clients" className={linkClass} onClick={onClose}>Clients</NavLink></li>
        <li><NavLink to="/treatments" className={linkClass} onClick={onClose}>Treatments</NavLink></li>
        <li><NavLink to="/treatment-types" className={linkClass} onClick={onClose}>Treatment Types</NavLink></li>
        <li><NavLink to="/follow-ups" className={linkClass} onClick={onClose}>Follow-Ups</NavLink></li>

        {isAdmin && (
          <>
            <li className="mt-3 mb-1"><small className="text-muted text-uppercase">Settings</small></li>
            <li><NavLink to="/settings/branding" className={linkClass} onClick={onClose}>Branding</NavLink></li>
          </>
        )}

        {isSuperAdmin && (
          <>
            <li className="mt-3 mb-1"><small className="text-muted text-uppercase">Admin</small></li>
            <li><NavLink to="/admin/companies" className={linkClass} onClick={onClose}>Companies</NavLink></li>
            <li><NavLink to="/admin/users" className={linkClass} onClick={onClose}>Users</NavLink></li>
            <li><NavLink to="/admin/audit-logs" className={linkClass} onClick={onClose}>Audit Logs</NavLink></li>
            <li><NavLink to="/admin/login-history" className={linkClass} onClick={onClose}>Login History</NavLink></li>
          </>
        )}
      </ul>
    </nav>
  )
}


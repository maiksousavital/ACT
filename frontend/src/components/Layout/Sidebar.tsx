import { useState } from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import { Collapse } from 'react-bootstrap'
import { useAuth } from '../../contexts/AuthContext'
import styles from './AppShell.module.css'

interface SidebarProps {
  isOpen: boolean
  onClose: () => void
}

export function Sidebar({ isOpen, onClose }: SidebarProps) {
  const { isSuperAdmin, isAdmin } = useAuth()
  const location = useLocation()

  const [settingsOpen, setSettingsOpen] = useState(location.pathname.startsWith('/settings'))
  const [adminOpen, setAdminOpen] = useState(location.pathname.startsWith('/admin'))

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `${styles.navLink} ${isActive ? styles.navLinkActive : ''}`

  return (
    <nav
      aria-label="Main navigation"
      className={`${styles.sidebar} ${isOpen ? styles.sidebarOpen : ''} d-lg-block p-3`}
      style={{ backgroundColor: 'var(--act-sidebar-bg, #1E293B)', color: 'var(--act-sidebar-text, #fff)' }}
    >
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h5 className="mb-0 fw-bold" style={{ color: 'var(--act-sidebar-text, #fff)' }}>ACT</h5>
        <button
          className="btn btn-sm btn-outline-light d-lg-none"
          onClick={onClose}
          aria-label="Close navigation menu"
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
          <li className="mt-3">
            <button
              className={`${styles.navLink} ${styles.menuToggle} w-100 border-0 bg-transparent text-start d-flex justify-content-between align-items-center`}
              onClick={() => setSettingsOpen(!settingsOpen)}
              aria-expanded={settingsOpen}
              aria-controls="settings-submenu"
            >
              <span>Settings</span>
              <span className={`${styles.chevron} ${settingsOpen ? styles.chevronOpen : ''}`}>›</span>
            </button>
            <Collapse in={settingsOpen}>
              <ul id="settings-submenu" className="nav flex-column gap-1 ms-3 mt-1">
                <li><NavLink to="/settings/branding" className={linkClass} onClick={onClose}>Branding</NavLink></li>
              </ul>
            </Collapse>
          </li>
        )}

        {isSuperAdmin && (
          <li className="mt-2">
            <button
              className={`${styles.navLink} ${styles.menuToggle} w-100 border-0 bg-transparent text-start d-flex justify-content-between align-items-center`}
              onClick={() => setAdminOpen(!adminOpen)}
              aria-expanded={adminOpen}
              aria-controls="admin-submenu"
            >
              <span>Admin</span>
              <span className={`${styles.chevron} ${adminOpen ? styles.chevronOpen : ''}`}>›</span>
            </button>
            <Collapse in={adminOpen}>
              <ul id="admin-submenu" className="nav flex-column gap-1 ms-3 mt-1">
                <li><NavLink to="/admin/companies" className={linkClass} onClick={onClose}>Companies</NavLink></li>
                <li><NavLink to="/admin/users" className={linkClass} onClick={onClose}>Users</NavLink></li>
                <li><NavLink to="/admin/audit-logs" className={linkClass} onClick={onClose}>Audit Logs</NavLink></li>
                <li><NavLink to="/admin/login-history" className={linkClass} onClick={onClose}>Login History</NavLink></li>
              </ul>
            </Collapse>
          </li>
        )}
      </ul>
    </nav>
  )
}


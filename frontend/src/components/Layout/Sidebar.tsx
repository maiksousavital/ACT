import { useState, useEffect, useRef } from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import { Collapse } from 'react-bootstrap'
import { useAuth } from '../../contexts/AuthContext'
import { useBrand } from '../../contexts/BrandContext'
import styles from './AppShell.module.css'

interface SidebarProps {
  isOpen: boolean
  onClose: () => void
}

export function Sidebar({ isOpen, onClose }: SidebarProps) {
  const { isSuperAdmin, isAdmin } = useAuth()
  const { brand } = useBrand()
  const location = useLocation()
  const navRef = useRef<HTMLElement>(null)

  const [settingsOpen, setSettingsOpen] = useState(location.pathname.startsWith('/settings'))
  const [adminOpen, setAdminOpen] = useState(location.pathname.startsWith('/admin'))

  // Reactively read CSS variables for sidebar colors (updated by applyBrandToRoot during live preview)
  const [sidebarBg, setSidebarBg] = useState(brand?.sidebarColor || '#1E293B')
  const [sidebarText, setSidebarText] = useState('#fff')

  useEffect(() => {
    const root = document.documentElement
    const bg = getComputedStyle(root).getPropertyValue('--act-sidebar-bg').trim() || brand?.sidebarColor || '#1E293B'
    const text = getComputedStyle(root).getPropertyValue('--act-sidebar-text').trim() || '#fff'
    setSidebarBg(bg)
    setSidebarText(text)
  }, [brand])

  // Also listen to CSS variable changes during live preview via MutationObserver
  useEffect(() => {
    const readVars = () => {
      const root = document.documentElement
      const bg = getComputedStyle(root).getPropertyValue('--act-sidebar-bg').trim()
      const text = getComputedStyle(root).getPropertyValue('--act-sidebar-text').trim()
      if (bg) setSidebarBg(bg)
      if (text) setSidebarText(text)
    }

    const observer = new MutationObserver(readVars)
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['style'] })
    const styleEl = document.getElementById('act-brand-styles')
    if (styleEl) observer.observe(styleEl, { childList: true, characterData: true, subtree: true })

    // Polling fallback for live preview (style element may be created after mount)
    const interval = setInterval(readVars, 300)

    return () => {
      observer.disconnect()
      clearInterval(interval)
    }
  }, [])

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `${styles.navLink} ${isActive ? styles.navLinkActive : ''}`

  return (
    <nav
      ref={navRef}
      aria-label="Main navigation"
      className={`${styles.sidebar} ${isOpen ? styles.sidebarOpen : ''} d-lg-block p-3`}
      style={{ backgroundColor: sidebarBg, color: sidebarText }}
    >
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h5 className="mb-0 fw-bold" style={{ color: sidebarText }}>ACT</h5>
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

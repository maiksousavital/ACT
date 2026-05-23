import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import styles from './AppShell.module.css'

export function AppShell() {
  const [sidebarOpen, setSidebarOpen] = useState(false)

  return (
    <div className="d-flex">
      {sidebarOpen && (
        <div
          className={`d-lg-none ${styles.overlay}`}
          onClick={() => setSidebarOpen(false)}
        />
      )}

      <Sidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />

      <div className={`d-flex flex-column ${styles.content}`}>
        <TopBar onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
        <main className="p-3 p-md-4">
          <Outlet />
        </main>
      </div>
    </div>
  )
}


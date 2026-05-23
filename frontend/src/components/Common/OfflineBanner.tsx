import { useState, useEffect } from 'react'
import { Alert } from 'react-bootstrap'

export function OfflineBanner() {
  const [isOffline, setIsOffline] = useState(!navigator.onLine)

  useEffect(() => {
    const handleOffline = () => setIsOffline(true)
    const handleOnline = () => setIsOffline(false)

    window.addEventListener('offline', handleOffline)
    window.addEventListener('online', handleOnline)

    return () => {
      window.removeEventListener('offline', handleOffline)
      window.removeEventListener('online', handleOnline)
    }
  }, [])

  if (!isOffline) return null

  return (
    <Alert variant="warning" className="mb-0 rounded-0 text-center py-2 small">
      ⚡ You are offline. Some features may be unavailable.
    </Alert>
  )
}


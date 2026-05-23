import { useState, useEffect } from 'react'
import { Button, Toast, ToastContainer } from 'react-bootstrap'

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>
}

export function InstallPrompt() {
  const [deferredPrompt, setDeferredPrompt] = useState<BeforeInstallPromptEvent | null>(null)
  const [show, setShow] = useState(false)

  useEffect(() => {
    const handler = (e: Event) => {
      e.preventDefault()
      setDeferredPrompt(e as BeforeInstallPromptEvent)
      setShow(true)
    }

    window.addEventListener('beforeinstallprompt', handler)
    return () => window.removeEventListener('beforeinstallprompt', handler)
  }, [])

  const handleInstall = async () => {
    if (!deferredPrompt) return
    await deferredPrompt.prompt()
    const { outcome } = await deferredPrompt.userChoice
    if (outcome === 'accepted') {
      setShow(false)
    }
    setDeferredPrompt(null)
  }

  if (!show) return null

  return (
    <ToastContainer position="bottom-center" className="p-3">
      <Toast onClose={() => setShow(false)} show={show}>
        <Toast.Header>
          <strong className="me-auto">Install ACT</strong>
        </Toast.Header>
        <Toast.Body className="d-flex align-items-center justify-content-between">
          <span className="small">Install ACT for quick access from your home screen.</span>
          <Button variant="primary" size="sm" onClick={handleInstall} className="ms-2">
            Install
          </Button>
        </Toast.Body>
      </Toast>
    </ToastContainer>
  )
}


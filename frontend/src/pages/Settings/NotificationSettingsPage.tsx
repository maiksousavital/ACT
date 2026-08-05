import { useEffect, useState } from 'react'
import { Card, Button, Alert, Spinner } from 'react-bootstrap'
import toast from 'react-hot-toast'
import {
  isPushSupported,
  getCurrentSubscription,
  enablePushNotifications,
  disablePushNotifications,
} from '../../utils/pushNotifications'

export function NotificationSettingsPage() {
  const [supported, setSupported] = useState(true)
  const [subscribed, setSubscribed] = useState(false)
  const [loading, setLoading] = useState(true)
  const [working, setWorking] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function checkStatus() {
      if (!isPushSupported()) {
        setSupported(false)
        setLoading(false)
        return
      }
      const subscription = await getCurrentSubscription()
      setSubscribed(!!subscription)
      setLoading(false)
    }
    checkStatus()
  }, [])

  const handleEnable = async () => {
    setError(null)
    setWorking(true)
    try {
      await enablePushNotifications()
      setSubscribed(true)
      toast.success('Notifications enabled')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not enable notifications.')
    } finally {
      setWorking(false)
    }
  }

  const handleDisable = async () => {
    setError(null)
    setWorking(true)
    try {
      await disablePushNotifications()
      setSubscribed(false)
      toast.success('Notifications disabled')
    } catch {
      setError('Could not disable notifications. Please try again.')
    } finally {
      setWorking(false)
    }
  }

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    )
  }

  return (
    <div>
      <h4 className="fw-bold mb-3">Notifications</h4>

      <Card className="border-0 shadow-sm" style={{ maxWidth: '600px' }}>
        <Card.Body className="p-3 p-md-4">
          {error && <Alert variant="danger" className="py-2">{error}</Alert>}

          {!supported ? (
            <Alert variant="warning" className="mb-0">
              This browser doesn&rsquo;t support push notifications. On iPhone/iPad, open this site
              in Safari, tap the Share icon, and choose &ldquo;Add to Home Screen&rdquo; — notifications
              only work once it&rsquo;s installed that way.
            </Alert>
          ) : (
            <>
              <p className="text-muted">
                Get notified when a client has a follow-up due — even when ACT isn&rsquo;t open. Once a day,
                you&rsquo;ll get a reminder listing anything outstanding.
              </p>
              <div className="d-flex align-items-center gap-3">
                {subscribed ? (
                  <Button variant="outline-danger" onClick={handleDisable} disabled={working}>
                    {working ? <Spinner animation="border" size="sm" /> : 'Disable notifications'}
                  </Button>
                ) : (
                  <Button variant="primary" onClick={handleEnable} disabled={working}>
                    {working ? <Spinner animation="border" size="sm" /> : 'Enable notifications'}
                  </Button>
                )}
                <span className="text-muted small">
                  {subscribed ? 'Notifications are on for this device.' : 'Notifications are off for this device.'}
                </span>
              </div>
            </>
          )}
        </Card.Body>
      </Card>
    </div>
  )
}

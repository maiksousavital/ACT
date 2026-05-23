import { useNavigate } from 'react-router-dom'
import { Button, Card } from 'react-bootstrap'

export function NotFoundPage() {
  const navigate = useNavigate()

  return (
    <div className="d-flex justify-content-center align-items-center vh-100 p-3">
      <Card className="border-0 shadow text-center" style={{ maxWidth: '480px' }}>
        <Card.Body className="p-4 p-md-5">
          <div className="fs-1 mb-3">🔍</div>
          <h1 className="fw-bold display-4 mb-2">404</h1>
          <h5 className="text-muted mb-4">Page not found</h5>
          <p className="text-muted mb-4">
            The page you're looking for doesn't exist or has been moved.
          </p>
          <div className="d-flex gap-2 justify-content-center">
            <Button variant="primary" onClick={() => navigate('/')}>
              Go to Dashboard
            </Button>
            <Button variant="outline-secondary" onClick={() => navigate(-1)}>
              Go Back
            </Button>
          </div>
        </Card.Body>
      </Card>
    </div>
  )
}


import { Component, type ReactNode, type ErrorInfo } from 'react'
import { Button, Card } from 'react-bootstrap'

interface Props {
  children: ReactNode
}

interface State {
  hasError: boolean
  error: Error | null
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false, error: null }
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught:', error, errorInfo)
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null })
    window.location.href = '/'
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="d-flex justify-content-center align-items-center vh-100 p-3">
          <Card className="border-0 shadow text-center" style={{ maxWidth: '500px' }}>
            <Card.Body className="p-4 p-md-5">
              <div className="fs-1 mb-3">😵</div>
              <h4 className="fw-bold mb-2">Something went wrong</h4>
              <p className="text-muted mb-4">
                An unexpected error occurred. Please try again or contact support if the problem persists.
              </p>
              {this.state.error && (
                <p className="small text-danger bg-danger bg-opacity-10 rounded p-2 mb-4">
                  {this.state.error.message}
                </p>
              )}
              <div className="d-flex gap-2 justify-content-center">
                <Button variant="primary" onClick={this.handleReset}>
                  Go to Dashboard
                </Button>
                <Button variant="outline-secondary" onClick={() => window.location.reload()}>
                  Reload Page
                </Button>
              </div>
            </Card.Body>
          </Card>
        </div>
      )
    }

    return this.props.children
  }
}


import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Button, Card, Alert, Spinner } from 'react-bootstrap'
import { authApi } from '../../api/authApi'
import styles from './LoginPage.module.css'

const forgotPasswordSchema = z.object({
  email: z.string().email('Please enter a valid email'),
})

type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>

export function ForgotPasswordPage() {
  const [submitted, setSubmitted] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  })

  const onSubmit = async (data: ForgotPasswordFormData) => {
    setError(null)
    setLoading(true)
    try {
      await authApi.forgotPassword(data.email)
      // Always show the same confirmation, whether or not the email is registered — the API
      // itself never reveals account existence, and the UI shouldn't either.
      setSubmitted(true)
    } catch {
      setError('Unable to connect to the server. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={`d-flex justify-content-center align-items-center p-3 ${styles.loginPage}`}>
      <Card className={`shadow ${styles.loginCard}`}>
        <Card.Body className="p-4 p-md-5">
          <div className="text-center mb-4">
            <h2 className="fw-bold" style={{ color: 'var(--bs-primary)' }}>ACT</h2>
            <p className="text-muted">Reset your password</p>
          </div>

          {submitted ? (
            <>
              <Alert variant="success" className="py-2">
                If an account exists for that email, we&rsquo;ve sent a link to reset your password. It expires in 30 minutes.
              </Alert>
              <div className="text-center mt-3">
                <Link to="/login">Back to sign in</Link>
              </div>
            </>
          ) : (
            <>
              {error && <Alert variant="danger" className="py-2">{error}</Alert>}
              <Form onSubmit={handleSubmit(onSubmit)}>
                <Form.Group className="mb-4">
                  <Form.Label>Email</Form.Label>
                  <Form.Control
                    type="email"
                    placeholder="you@example.com"
                    isInvalid={!!errors.email}
                    {...register('email')}
                  />
                  <Form.Control.Feedback type="invalid">
                    {errors.email?.message}
                  </Form.Control.Feedback>
                </Form.Group>

                <Button type="submit" variant="primary" className="w-100" disabled={loading}>
                  {loading ? <Spinner animation="border" size="sm" /> : 'Send reset link'}
                </Button>
              </Form>
              <div className="text-center mt-3">
                <Link to="/login">Back to sign in</Link>
              </div>
            </>
          )}
        </Card.Body>
      </Card>
    </div>
  )
}

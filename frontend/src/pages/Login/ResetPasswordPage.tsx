import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Button, Card, Alert, Spinner } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { authApi } from '../../api/authApi'
import styles from './LoginPage.module.css'

// Mirrors the backend's ResetPasswordRequest.NewPassword validation exactly, so a mismatch is
// caught client-side instead of round-tripping to the server.
const resetPasswordSchema = z
  .object({
    newPassword: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .regex(/(?=.*[A-Za-z])(?=.*\d)/, 'Password must include at least one letter and one number'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>

function extractErrorMessage(err: unknown, fallback: string): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const axiosErr = err as { response?: { data?: { detail?: string; message?: string } } }
    return axiosErr.response?.data?.detail || axiosErr.response?.data?.message || fallback
  }
  return fallback
}

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  })

  const onSubmit = async (data: ResetPasswordFormData) => {
    if (!token) return
    setError(null)
    setLoading(true)
    try {
      await authApi.resetPassword(token, data.newPassword)
      toast.success('Password reset — please sign in with your new password.')
      navigate('/login', { replace: true })
    } catch (err: unknown) {
      setError(extractErrorMessage(err, 'Unable to reset your password. Please try again.'))
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
            <p className="text-muted">Choose a new password</p>
          </div>

          {!token ? (
            <>
              <Alert variant="danger" className="py-2">
                This reset link is missing or invalid.
              </Alert>
              <div className="text-center mt-3">
                <Link to="/forgot-password">Request a new link</Link>
              </div>
            </>
          ) : (
            <>
              {error && <Alert variant="danger" className="py-2">{error}</Alert>}
              <Form onSubmit={handleSubmit(onSubmit)}>
                <Form.Group className="mb-3">
                  <Form.Label>New password</Form.Label>
                  <Form.Control
                    type="password"
                    placeholder="••••••••"
                    isInvalid={!!errors.newPassword}
                    {...register('newPassword')}
                  />
                  <Form.Control.Feedback type="invalid">
                    {errors.newPassword?.message}
                  </Form.Control.Feedback>
                </Form.Group>

                <Form.Group className="mb-4">
                  <Form.Label>Confirm new password</Form.Label>
                  <Form.Control
                    type="password"
                    placeholder="••••••••"
                    isInvalid={!!errors.confirmPassword}
                    {...register('confirmPassword')}
                  />
                  <Form.Control.Feedback type="invalid">
                    {errors.confirmPassword?.message}
                  </Form.Control.Feedback>
                </Form.Group>

                <Button type="submit" variant="primary" className="w-100" disabled={loading}>
                  {loading ? <Spinner animation="border" size="sm" /> : 'Reset password'}
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

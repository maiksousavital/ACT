import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Button, Card, Spinner, Alert } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { userApi } from '../../../api/userApi'
import { companyApi } from '../../../api/companyApi'
import { useAuth } from '../../../contexts/AuthContext'
import type { CompanyDto } from '../../../types/company'

const userSchema = z.object({
  email: z.string().email('Valid email is required'),
  password: z.string().min(6, 'Min 6 characters'),
  companyId: z.string().min(1, 'Select a company'),
  role: z.enum(['Admin', 'User']),
})

type UserFormData = z.infer<typeof userSchema>

export function UserFormPage() {
  const navigate = useNavigate()
  const { isSuperAdmin, user } = useAuth()
  const [loading, setLoading] = useState(false)
  const [fetching, setFetching] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [companies, setCompanies] = useState<CompanyDto[]>([])

  const { register, handleSubmit, formState: { errors } } = useForm<UserFormData>({
    resolver: zodResolver(userSchema),
    defaultValues: {
      role: 'Admin',
      companyId: user?.companyId ? String(user.companyId) : '',
    },
  })

  useEffect(() => {
    if (isSuperAdmin) {
      companyApi.getPaged(1, 100).then((res) => {
        setCompanies(res.items)
        setFetching(false)
      }).catch(() => setFetching(false))
    } else {
      setFetching(false)
    }
  }, [isSuperAdmin])

  const onSubmit = async (data: UserFormData) => {
    setLoading(true)
    setError(null)
    try {
      await userApi.create({
        email: data.email,
        password: data.password,
        companyId: Number(data.companyId),
        role: data.role,
      })
      toast.success('User created')
      navigate('/admin/users')
    } catch {
      setError('Failed to create user. Email may already exist.')
    } finally {
      setLoading(false)
    }
  }

  if (fetching) return <div className="text-center py-5"><Spinner animation="border" variant="primary" /></div>

  return (
    <div>
      <h4 className="fw-bold mb-3">New User</h4>
      <Card className="border-0 shadow-sm">
        <Card.Body className="p-3 p-md-4">
          {error && <Alert variant="danger">{error}</Alert>}
          <Form onSubmit={handleSubmit(onSubmit)} className="mx-auto" style={{ maxWidth: '600px' }}>
            <Form.Group className="mb-3">
              <Form.Label>Email *</Form.Label>
              <Form.Control type="email" isInvalid={!!errors.email} {...register('email')} />
              <Form.Control.Feedback type="invalid">{errors.email?.message}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Password *</Form.Label>
              <Form.Control type="password" isInvalid={!!errors.password} {...register('password')} />
              <Form.Control.Feedback type="invalid">{errors.password?.message}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Company *</Form.Label>
              <Form.Select
                isInvalid={!!errors.companyId}
                disabled={!isSuperAdmin}
                {...register('companyId')}
              >
                <option value="">Select company...</option>
                {isSuperAdmin ? (
                  companies.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)
                ) : (
                  user?.companyId && <option value={user.companyId}>My Company</option>
                )}
              </Form.Select>
              <Form.Control.Feedback type="invalid">{errors.companyId?.message}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Role *</Form.Label>
              <Form.Select {...register('role')}>
                <option value="Admin">Admin</option>
                <option value="User">User</option>
              </Form.Select>
            </Form.Group>
            <div className="d-flex gap-2">
              <Button type="submit" variant="primary" disabled={loading}>
                {loading ? <Spinner animation="border" size="sm" /> : 'Create User'}
              </Button>
              <Button variant="outline-secondary" onClick={() => navigate('/admin/users')}>Cancel</Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </div>
  )
}


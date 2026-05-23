import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Button, Card, Spinner, Alert } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { userApi } from '../../../api/userApi'
import { companyApi } from '../../../api/companyApi'
import { useAuth } from '../../../contexts/AuthContext'
import type { CompanyDto } from '../../../types/company'

const createSchema = z.object({
  email: z.string().email('Valid email is required'),
  password: z.string().min(6, 'Min 6 characters'),
  companyId: z.string().min(1, 'Select a company'),
  role: z.enum(['Admin', 'User']),
})

const editSchema = z.object({
  email: z.string().email('Valid email is required'),
  companyId: z.string().min(1, 'Select a company'),
  role: z.enum(['Admin', 'User']),
  isActive: z.boolean(),
})

type CreateFormData = z.infer<typeof createSchema>
type EditFormData = z.infer<typeof editSchema>

export function UserFormPage() {
  const { id } = useParams()
  const isEdit = !!id
  const navigate = useNavigate()
  const { isSuperAdmin, user } = useAuth()
  const [loading, setLoading] = useState(false)
  const [fetching, setFetching] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [companies, setCompanies] = useState<CompanyDto[]>([])
  const [userEmail, setUserEmail] = useState('')

  const createForm = useForm<CreateFormData>({
    resolver: zodResolver(createSchema),
    defaultValues: {
      role: 'Admin',
      companyId: user?.companyId ? String(user.companyId) : '',
    },
  })

  const editForm = useForm<EditFormData>({
    resolver: zodResolver(editSchema),
    defaultValues: { email: '', companyId: '', role: 'Admin', isActive: true },
  })

  useEffect(() => {
    async function fetchData() {
      if (isSuperAdmin) {
        try {
          const res = await companyApi.getPaged(1, 100)
          setCompanies(res.items)
        } catch { /* */ }
      }

      if (isEdit) {
        try {
          const u = await userApi.getById(Number(id))
          setUserEmail(u.email)
          editForm.reset({
            email: u.email,
            companyId: u.companyId ? String(u.companyId) : '',
            role: u.role as 'Admin' | 'User',
            isActive: u.isActive,
          })
        } catch { /* */ }
      }

      setFetching(false)
    }
    fetchData()
  }, [id, isEdit, isSuperAdmin, editForm])

  const onCreateSubmit = async (data: CreateFormData) => {
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

  const onEditSubmit = async (data: EditFormData) => {
    setLoading(true)
    setError(null)
    try {
      await userApi.update(Number(id), {
        email: data.email,
        companyId: Number(data.companyId),
        role: data.role,
        isActive: data.isActive,
      })
      toast.success('User updated')
      navigate('/admin/users')
    } catch {
      setError('Failed to update user.')
    } finally {
      setLoading(false)
    }
  }

  if (fetching) return <div className="text-center py-5"><Spinner animation="border" variant="primary" /></div>

  return (
    <div>
      <h4 className="fw-bold mb-3">{isEdit ? `Edit User — ${userEmail}` : 'New User'}</h4>
      <Card className="border-0 shadow-sm">
        <Card.Body className="p-3 p-md-4">
          {error && <Alert variant="danger">{error}</Alert>}

          {isEdit ? (
            <Form onSubmit={editForm.handleSubmit(onEditSubmit)} className="mx-auto" style={{ maxWidth: '600px' }}>
              <Form.Group className="mb-3">
                <Form.Label>Email *</Form.Label>
                <Form.Control type="email" isInvalid={!!editForm.formState.errors.email} {...editForm.register('email')} />
                <Form.Control.Feedback type="invalid">{editForm.formState.errors.email?.message}</Form.Control.Feedback>
              </Form.Group>
              <Form.Group className="mb-3">
                <Form.Label>Company *</Form.Label>
                <Form.Select
                  isInvalid={!!editForm.formState.errors.companyId}
                  disabled={!isSuperAdmin}
                  {...editForm.register('companyId')}
                >
                  <option value="">Select company...</option>
                  {isSuperAdmin ? (
                    companies.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)
                  ) : (
                    user?.companyId && <option value={user.companyId}>My Company</option>
                  )}
                </Form.Select>
                <Form.Control.Feedback type="invalid">{editForm.formState.errors.companyId?.message}</Form.Control.Feedback>
              </Form.Group>
              <Form.Group className="mb-3">
                <Form.Label>Role *</Form.Label>
                <Form.Select {...editForm.register('role')}>
                  <option value="Admin">Admin</option>
                  <option value="User">User</option>
                </Form.Select>
              </Form.Group>
              <Form.Group className="mb-3">
                <Form.Check
                  type="switch"
                  label="Active"
                  {...editForm.register('isActive')}
                />
              </Form.Group>
              <div className="d-flex gap-2">
                <Button type="submit" variant="primary" disabled={loading}>
                  {loading ? <Spinner animation="border" size="sm" /> : 'Save Changes'}
                </Button>
                <Button variant="outline-secondary" onClick={() => navigate('/admin/users')}>Cancel</Button>
              </div>
            </Form>
          ) : (
            <Form onSubmit={createForm.handleSubmit(onCreateSubmit)} className="mx-auto" style={{ maxWidth: '600px' }}>
              <Form.Group className="mb-3">
                <Form.Label>Email *</Form.Label>
                <Form.Control type="email" isInvalid={!!createForm.formState.errors.email} {...createForm.register('email')} />
                <Form.Control.Feedback type="invalid">{createForm.formState.errors.email?.message}</Form.Control.Feedback>
              </Form.Group>
              <Form.Group className="mb-3">
                <Form.Label>Password *</Form.Label>
                <Form.Control type="password" isInvalid={!!createForm.formState.errors.password} {...createForm.register('password')} />
                <Form.Control.Feedback type="invalid">{createForm.formState.errors.password?.message}</Form.Control.Feedback>
              </Form.Group>
              <Form.Group className="mb-3">
                <Form.Label>Company *</Form.Label>
                <Form.Select
                  isInvalid={!!createForm.formState.errors.companyId}
                  disabled={!isSuperAdmin}
                  {...createForm.register('companyId')}
                >
                  <option value="">Select company...</option>
                  {isSuperAdmin ? (
                    companies.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)
                  ) : (
                    user?.companyId && <option value={user.companyId}>My Company</option>
                  )}
                </Form.Select>
                <Form.Control.Feedback type="invalid">{createForm.formState.errors.companyId?.message}</Form.Control.Feedback>
              </Form.Group>
              <Form.Group className="mb-3">
                <Form.Label>Role *</Form.Label>
                <Form.Select {...createForm.register('role')}>
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
          )}
        </Card.Body>
      </Card>
    </div>
  )
}


import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Button, Card, Spinner, Alert } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { clientApi } from '../../api/clientApi'
import { companyApi } from '../../api/companyApi'
import { EntityAuditLogButton } from '../../components/Audit/EntityAuditLogButton'
import { useAuth } from '../../contexts/AuthContext'
import type { CompanyDto } from '../../types/company'

const clientSchema = z.object({
  companyId: z.string().optional(),
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  phone: z.string().optional(),
  notes: z.string().optional(),
})

type ClientFormData = z.infer<typeof clientSchema>

export function ClientFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = !!id
  const { isSuperAdmin } = useAuth()
  const [loading, setLoading] = useState(false)
  const [fetching, setFetching] = useState(isEdit)
  const [error, setError] = useState<string | null>(null)
  const [companies, setCompanies] = useState<CompanyDto[]>([])

  const {
    register,
    handleSubmit,
    reset,
    setError: setFieldError,
    formState: { errors },
  } = useForm<ClientFormData>({
    resolver: zodResolver(clientSchema),
  })

  useEffect(() => {
    if (isSuperAdmin && !isEdit) {
      companyApi.getPaged(1, 100).then((res) => setCompanies(res.items)).catch(() => {})
    }
  }, [isSuperAdmin, isEdit])

  useEffect(() => {
    if (isEdit) {
      clientApi.getById(Number(id)).then((client) => {
        reset({
          firstName: client.firstName,
          lastName: client.lastName,
          email: client.email || '',
          phone: client.phone || '',
          notes: client.notes || '',
        })
        setFetching(false)
      }).catch(() => {
        setError('Client not found')
        setFetching(false)
      })
    }
  }, [id, isEdit, reset])

  const onSubmit = async (data: ClientFormData) => {
    if (isSuperAdmin && !isEdit && !data.companyId) {
      setFieldError('companyId', { message: 'Select a company' })
      return
    }
    setLoading(true)
    setError(null)
    try {
      if (isEdit) {
        await clientApi.update(Number(id), data)
        toast.success('Client updated')
      } else {
        await clientApi.create({ ...data, companyId: data.companyId ? Number(data.companyId) : undefined })
        toast.success('Client created')
      }
      navigate('/clients')
    } catch {
      setError('Failed to save client. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  if (fetching) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    )
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h4 className="fw-bold mb-0">{isEdit ? 'Edit Client' : 'New Client'}</h4>
        {isEdit && <EntityAuditLogButton entityType="Client" entityId={Number(id)} />}
      </div>

      <Card className="border-0 shadow-sm">
        <Card.Body className="p-3 p-md-4">
          {error && <Alert variant="danger">{error}</Alert>}

          <Form onSubmit={handleSubmit(onSubmit)} className="mx-auto" style={{ maxWidth: '600px' }}>
            {isSuperAdmin && !isEdit && (
              <Form.Group className="mb-3">
                <Form.Label>Company *</Form.Label>
                <Form.Select isInvalid={!!errors.companyId} {...register('companyId')}>
                  <option value="">Select company...</option>
                  {companies.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </Form.Select>
                <Form.Control.Feedback type="invalid">{errors.companyId?.message}</Form.Control.Feedback>
              </Form.Group>
            )}

            <Form.Group className="mb-3">
              <Form.Label>First Name *</Form.Label>
              <Form.Control isInvalid={!!errors.firstName} {...register('firstName')} />
              <Form.Control.Feedback type="invalid">{errors.firstName?.message}</Form.Control.Feedback>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Last Name *</Form.Label>
              <Form.Control isInvalid={!!errors.lastName} {...register('lastName')} />
              <Form.Control.Feedback type="invalid">{errors.lastName?.message}</Form.Control.Feedback>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Email</Form.Label>
              <Form.Control type="email" isInvalid={!!errors.email} {...register('email')} />
              <Form.Control.Feedback type="invalid">{errors.email?.message}</Form.Control.Feedback>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Phone</Form.Label>
              <Form.Control {...register('phone')} />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Notes</Form.Label>
              <Form.Control as="textarea" rows={3} {...register('notes')} />
            </Form.Group>

            <div className="d-flex gap-2">
              <Button type="submit" variant="primary" disabled={loading}>
                {loading ? <Spinner animation="border" size="sm" /> : isEdit ? 'Update' : 'Create'}
              </Button>
              <Button variant="outline-secondary" onClick={() => navigate('/clients')}>
                Cancel
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </div>
  )
}


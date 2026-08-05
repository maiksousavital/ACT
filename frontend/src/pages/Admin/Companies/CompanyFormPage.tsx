import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Button, Card, Spinner, Alert } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { companyApi } from '../../../api/companyApi'
import { EntityAuditLogButton } from '../../../components/Audit/EntityAuditLogButton'

const companySchema = z.object({
  name: z.string().min(1, 'Company name is required'),
  contactEmail: z.string().email('Invalid email').optional().or(z.literal('')),
  phone: z.string().optional(),
  address: z.string().optional(),
})

type CompanyFormData = z.infer<typeof companySchema>

export function CompanyFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = !!id
  const [loading, setLoading] = useState(false)
  const [fetching, setFetching] = useState(isEdit)
  const [error, setError] = useState<string | null>(null)

  const { register, handleSubmit, reset, formState: { errors } } = useForm<CompanyFormData>({
    resolver: zodResolver(companySchema),
  })

  useEffect(() => {
    if (isEdit) {
      companyApi.getById(Number(id)).then((c) => {
        reset({ name: c.name, contactEmail: c.contactEmail || '', phone: c.phone || '', address: c.address || '' })
        setFetching(false)
      }).catch(() => { setError('Company not found'); setFetching(false) })
    }
  }, [id, isEdit, reset])

  const onSubmit = async (data: CompanyFormData) => {
    setLoading(true)
    setError(null)
    try {
      if (isEdit) {
        await companyApi.update(Number(id), data)
        toast.success('Company updated')
      } else {
        await companyApi.create(data)
        toast.success('Company created')
      }
      navigate('/admin/companies')
    } catch {
      setError('Failed to save. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  if (fetching) return <div className="text-center py-5"><Spinner animation="border" variant="primary" /></div>

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h4 className="fw-bold mb-0">{isEdit ? 'Edit Company' : 'New Company'}</h4>
        {isEdit && <EntityAuditLogButton entityType="Company" entityId={Number(id)} />}
      </div>
      <Card className="border-0 shadow-sm">
        <Card.Body className="p-3 p-md-4">
          {error && <Alert variant="danger">{error}</Alert>}
          <Form onSubmit={handleSubmit(onSubmit)} className="mx-auto" style={{ maxWidth: '600px' }}>
            <Form.Group className="mb-3">
              <Form.Label>Company Name *</Form.Label>
              <Form.Control isInvalid={!!errors.name} {...register('name')} />
              <Form.Control.Feedback type="invalid">{errors.name?.message}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Contact Email</Form.Label>
              <Form.Control type="email" isInvalid={!!errors.contactEmail} {...register('contactEmail')} />
              <Form.Control.Feedback type="invalid">{errors.contactEmail?.message}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Phone</Form.Label>
              <Form.Control {...register('phone')} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Address</Form.Label>
              <Form.Control as="textarea" rows={2} {...register('address')} />
            </Form.Group>
            <div className="d-flex gap-2">
              <Button type="submit" variant="primary" disabled={loading}>
                {loading ? <Spinner animation="border" size="sm" /> : isEdit ? 'Update' : 'Create'}
              </Button>
              <Button variant="outline-secondary" onClick={() => navigate('/admin/companies')}>Cancel</Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </div>
  )
}


import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Button, Card, Spinner, Alert } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { treatmentTypeApi } from '../../api/treatmentTypeApi'
import { EntityAuditLogButton } from '../../components/Audit/EntityAuditLogButton'
import type { FollowUpPeriodOption } from '../../types/treatmentType'

const treatmentTypeSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  followUpIntervalDays: z.string().min(1, 'Select a follow-up interval'),
})

type TreatmentTypeFormData = z.infer<typeof treatmentTypeSchema>

export function TreatmentTypeFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = !!id
  const [loading, setLoading] = useState(false)
  const [fetching, setFetching] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [periods, setPeriods] = useState<FollowUpPeriodOption[]>([])

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<TreatmentTypeFormData>({
    resolver: zodResolver(treatmentTypeSchema),
  })

  useEffect(() => {
    async function fetchMetadata() {
      try {
        let allPeriods = (await treatmentTypeApi.getMetadata()).allowedFollowUpPeriods

        if (isEdit) {
          const tt = await treatmentTypeApi.getById(Number(id))
          if (!allPeriods.some((p) => p.days === tt.followUpIntervalDays)) {
            allPeriods = [...allPeriods, { name: `${tt.followUpIntervalDays} days`, days: tt.followUpIntervalDays }]
              .sort((a, b) => a.days - b.days)
          }
          reset({ name: tt.name, followUpIntervalDays: String(tt.followUpIntervalDays) })
        }

        setPeriods(allPeriods)
      } catch {
        setError('Failed to load data')
      } finally {
        setFetching(false)
      }
    }
    fetchMetadata()
  }, [id, isEdit, reset])

  const onSubmit = async (data: TreatmentTypeFormData) => {
    setLoading(true)
    setError(null)
    const payload = { name: data.name, followUpIntervalDays: Number(data.followUpIntervalDays) }
    try {
      if (isEdit) {
        await treatmentTypeApi.update(Number(id), payload)
        toast.success('Treatment type updated')
      } else {
        await treatmentTypeApi.create(payload)
        toast.success('Treatment type created')
      }
      navigate('/treatment-types')
    } catch {
      setError('Failed to save. Please try again.')
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
        <h4 className="fw-bold mb-0">{isEdit ? 'Edit Treatment Type' : 'New Treatment Type'}</h4>
        {isEdit && <EntityAuditLogButton entityType="TreatmentType" entityId={Number(id)} />}
      </div>

      <Card className="border-0 shadow-sm">
        <Card.Body className="p-3 p-md-4">
          {error && <Alert variant="danger">{error}</Alert>}

          <Form onSubmit={handleSubmit(onSubmit)} className="mx-auto" style={{ maxWidth: '600px' }}>
            <Form.Group className="mb-3">
              <Form.Label>Name *</Form.Label>
              <Form.Control isInvalid={!!errors.name} {...register('name')} />
              <Form.Control.Feedback type="invalid">{errors.name?.message}</Form.Control.Feedback>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Follow-Up Interval *</Form.Label>
              <Form.Select isInvalid={!!errors.followUpIntervalDays} {...register('followUpIntervalDays')}>
                <option value="">Select interval...</option>
                {periods.map((p) => (
                  <option key={p.days} value={p.days}>{p.name}</option>
                ))}
              </Form.Select>
              <Form.Control.Feedback type="invalid">{errors.followUpIntervalDays?.message}</Form.Control.Feedback>
            </Form.Group>

            <div className="d-flex gap-2">
              <Button type="submit" variant="primary" disabled={loading}>
                {loading ? <Spinner animation="border" size="sm" /> : isEdit ? 'Update' : 'Create'}
              </Button>
              <Button variant="outline-secondary" onClick={() => navigate('/treatment-types')}>
                Cancel
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </div>
  )
}


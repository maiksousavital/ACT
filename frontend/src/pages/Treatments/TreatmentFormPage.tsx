import { useEffect, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Button, Card, Spinner, Alert } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { treatmentApi } from '../../api/treatmentApi'
import { clientApi } from '../../api/clientApi'
import { treatmentTypeApi } from '../../api/treatmentTypeApi'
import type { ClientDto } from '../../types/client'
import type { TreatmentTypeDto } from '../../types/treatmentType'

const treatmentSchema = z.object({
  clientId: z.string().min(1, 'Select a client'),
  treatmentTypeId: z.string().min(1, 'Select a treatment type'),
  treatmentDate: z.string().min(1, 'Date is required'),
  notes: z.string().optional(),
})

type TreatmentFormData = z.infer<typeof treatmentSchema>

export function TreatmentFormPage() {
  const { id } = useParams()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const isEdit = !!id
  const [loading, setLoading] = useState(false)
  const [fetching, setFetching] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [clients, setClients] = useState<ClientDto[]>([])
  const [treatmentTypes, setTreatmentTypes] = useState<TreatmentTypeDto[]>([])

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<TreatmentFormData>({
    resolver: zodResolver(treatmentSchema),
    defaultValues: {
      clientId: searchParams.get('clientId') || '',
      treatmentDate: new Date().toISOString().split('T')[0],
    },
  })

  useEffect(() => {
    async function fetchMetadata() {
      try {
        const [clientsRes, typesRes] = await Promise.all([
          clientApi.getPaged(1, 100),
          treatmentTypeApi.getPaged(1, 100),
        ])
        setClients(clientsRes.items)
        setTreatmentTypes(typesRes.items)

        if (isEdit) {
          const t = await treatmentApi.getById(Number(id))
          reset({
            clientId: String(t.clientId),
            treatmentTypeId: String(t.treatmentTypeId),
            treatmentDate: t.treatmentDate.split('T')[0],
            notes: t.notes || '',
          })
        }
      } catch {
        setError('Failed to load data')
      } finally {
        setFetching(false)
      }
    }
    fetchMetadata()
  }, [id, isEdit, reset, searchParams])

  const onSubmit = async (data: TreatmentFormData) => {
    setLoading(true)
    setError(null)
    try {
      if (isEdit) {
        await treatmentApi.update(Number(id), {
          treatmentDate: data.treatmentDate,
          notes: data.notes,
        })
        toast.success('Treatment updated')
      } else {
        await treatmentApi.create({
          clientId: Number(data.clientId),
          treatmentTypeId: Number(data.treatmentTypeId),
          treatmentDate: data.treatmentDate,
          notes: data.notes,
        })
        toast.success('Treatment created')
      }
      navigate('/treatments')
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
      <h4 className="fw-bold mb-3">{isEdit ? 'Edit Treatment' : 'New Treatment'}</h4>

      <Card className="border-0 shadow-sm">
        <Card.Body className="p-3 p-md-4">
          {error && <Alert variant="danger">{error}</Alert>}

          <Form onSubmit={handleSubmit(onSubmit)} className="mx-auto" style={{ maxWidth: '600px' }}>
            <Form.Group className="mb-3">
              <Form.Label>Client *</Form.Label>
              <Form.Select
                isInvalid={!!errors.clientId}
                disabled={isEdit}
                {...register('clientId')}
              >
                <option value="">Select client...</option>
                {clients.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.firstName} {c.lastName}
                  </option>
                ))}
              </Form.Select>
              <Form.Control.Feedback type="invalid">{errors.clientId?.message}</Form.Control.Feedback>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Treatment Type *</Form.Label>
              <Form.Select
                isInvalid={!!errors.treatmentTypeId}
                disabled={isEdit}
                {...register('treatmentTypeId')}
              >
                <option value="">Select treatment type...</option>
                {treatmentTypes.map((tt) => (
                  <option key={tt.id} value={tt.id}>{tt.name}</option>
                ))}
              </Form.Select>
              <Form.Control.Feedback type="invalid">{errors.treatmentTypeId?.message}</Form.Control.Feedback>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Treatment Date *</Form.Label>
              <Form.Control type="date" isInvalid={!!errors.treatmentDate} {...register('treatmentDate')} />
              <Form.Control.Feedback type="invalid">{errors.treatmentDate?.message}</Form.Control.Feedback>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Notes</Form.Label>
              <Form.Control as="textarea" rows={3} {...register('notes')} />
            </Form.Group>

            <div className="d-flex gap-2">
              <Button type="submit" variant="primary" disabled={loading}>
                {loading ? <Spinner animation="border" size="sm" /> : isEdit ? 'Update' : 'Create'}
              </Button>
              <Button variant="outline-secondary" onClick={() => navigate('/treatments')}>
                Cancel
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </div>
  )
}


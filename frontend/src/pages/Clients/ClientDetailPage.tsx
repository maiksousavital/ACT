import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Card, Button, Spinner, Table, Badge } from 'react-bootstrap'
import { clientApi } from '../../api/clientApi'
import type { ClientDto } from '../../types/client'
import type { TreatmentDto } from '../../types/treatment'
import axiosInstance from '../../api/axiosInstance'

export function ClientDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [client, setClient] = useState<ClientDto | null>(null)
  const [treatments, setTreatments] = useState<TreatmentDto[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function fetch() {
      try {
        const c = await clientApi.getById(Number(id))
        setClient(c)
        // fetch treatments for this client
        const res = await axiosInstance.get<TreatmentDto[]>(`/treatment/by-client/${id}`)
        setTreatments(res.data)
      } catch {
        // not found
      } finally {
        setLoading(false)
      }
    }
    fetch()
  }, [id])

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    )
  }

  if (!client) {
    return <p className="text-muted text-center py-5">Client not found.</p>
  }

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center mb-3 gap-2">
        <h4 className="fw-bold mb-0">{client.firstName} {client.lastName}</h4>
        <div className="d-flex gap-2">
          <Button variant="outline-primary" size="sm" onClick={() => navigate(`/clients/${id}/edit`)}>
            Edit
          </Button>
          <Button variant="primary" size="sm" onClick={() => navigate(`/treatments/new?clientId=${id}`)}>
            + Add Treatment
          </Button>
          <Button variant="outline-secondary" size="sm" onClick={() => navigate('/clients')}>
            Back
          </Button>
        </div>
      </div>

      {/* Client Info */}
      <Card className="border-0 shadow-sm mb-4">
        <Card.Body>
          <div className="row g-3">
            <div className="col-12 col-sm-6">
              <small className="text-muted d-block">Email</small>
              <span>{client.email || '—'}</span>
            </div>
            <div className="col-12 col-sm-6">
              <small className="text-muted d-block">Phone</small>
              <span>{client.phone || '—'}</span>
            </div>
            <div className="col-12 col-sm-6">
              <small className="text-muted d-block">Status</small>
              <Badge bg={client.isArchived ? 'secondary' : 'success'}>
                {client.isArchived ? 'Archived' : 'Active'}
              </Badge>
            </div>
            <div className="col-12 col-sm-6">
              <small className="text-muted d-block">Created</small>
              <span>{new Date(client.createdAt).toLocaleDateString()}</span>
            </div>
            {client.notes && (
              <div className="col-12">
                <small className="text-muted d-block">Notes</small>
                <span>{client.notes}</span>
              </div>
            )}
          </div>
        </Card.Body>
      </Card>

      {/* Treatments */}
      <Card className="border-0 shadow-sm">
        <Card.Header className="bg-white border-bottom">
          <h6 className="mb-0 fw-semibold">Treatments ({treatments.length})</h6>
        </Card.Header>
        <Card.Body className="p-0">
          {treatments.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No treatments yet.</p>
          ) : (
            <div className="table-responsive">
              <Table hover className="mb-0 align-middle">
                <thead className="table-light">
                  <tr>
                    <th>Treatment Type</th>
                    <th className="d-none d-md-table-cell">Date</th>
                    <th>Next Follow-Up</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {treatments.map((t) => (
                    <tr key={t.id}>
                      <td>{t.treatmentTypeName}</td>
                      <td className="d-none d-md-table-cell">{new Date(t.treatmentDate).toLocaleDateString()}</td>
                      <td>{new Date(t.nextFollowUpDate).toLocaleDateString()}</td>
                      <td>
                        {t.isFollowedUp ? (
                          <Badge bg="success">Completed</Badge>
                        ) : t.isDue ? (
                          <Badge bg="danger">Overdue</Badge>
                        ) : (
                          <Badge bg="info">Upcoming</Badge>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </div>
          )}
        </Card.Body>
      </Card>
    </div>
  )
}


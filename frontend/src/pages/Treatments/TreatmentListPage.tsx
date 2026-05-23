import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Table, Button, Card, Spinner, Badge } from 'react-bootstrap'
import { treatmentApi } from '../../api/treatmentApi'
import { Pagination } from '../../components/Table/Pagination'
import type { TreatmentDto } from '../../types/treatment'
import type { PagedResult } from '../../types/common'

export function TreatmentListPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<PagedResult<TreatmentDto> | null>(null)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 10

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      const result = await treatmentApi.getPaged(page, pageSize)
      setData(result)
    } catch {
      // silently fail
    } finally {
      setLoading(false)
    }
  }, [page])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  const getStatusBadge = (t: TreatmentDto) => {
    if (t.isFollowedUp) return <Badge bg="success">Completed</Badge>
    if (t.isDue) return <Badge bg="danger">Overdue</Badge>
    return <Badge bg="info">Upcoming</Badge>
  }

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center mb-3 gap-2">
        <h4 className="fw-bold mb-0">Treatments</h4>
        <Button variant="primary" size="sm" onClick={() => navigate('/treatments/new')}>
          + Add Treatment
        </Button>
      </div>

      <Card className="border-0 shadow-sm">
        <Card.Body>
          {loading ? (
            <div className="text-center py-4">
              <Spinner animation="border" variant="primary" size="sm" />
            </div>
          ) : !data || data.items.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No treatments found.</p>
          ) : (
            <>
              <div className="table-responsive">
                <Table hover className="mb-0 align-middle">
                  <thead className="table-light">
                    <tr>
                      <th>Client</th>
                      <th className="d-none d-md-table-cell">Treatment Type</th>
                      <th className="d-none d-sm-table-cell">Date</th>
                      <th>Next Follow-Up</th>
                      <th>Status</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((t) => (
                      <tr key={t.id}>
                        <td className="fw-medium">{t.clientName}</td>
                        <td className="d-none d-md-table-cell">{t.treatmentTypeName}</td>
                        <td className="d-none d-sm-table-cell">{new Date(t.treatmentDate).toLocaleDateString()}</td>
                        <td>{new Date(t.nextFollowUpDate).toLocaleDateString()}</td>
                        <td>{getStatusBadge(t)}</td>
                        <td>
                          <Button
                            variant="outline-primary"
                            size="sm"
                            onClick={() => navigate(`/treatments/${t.id}/edit`)}
                          >
                            Edit
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>

              {data && (
                <div className="d-flex justify-content-between align-items-center mt-3">
                  <small className="text-muted">
                    Showing {data.items.length} of {data.totalCount}
                  </small>
                  <Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} />
                </div>
              )}
            </>
          )}
        </Card.Body>
      </Card>
    </div>
  )
}


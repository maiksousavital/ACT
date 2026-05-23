import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Table, Button, Card, Spinner, Badge } from 'react-bootstrap'
import { treatmentTypeApi } from '../../api/treatmentTypeApi'
import { Pagination } from '../../components/Table/Pagination'
import type { TreatmentTypeDto } from '../../types/treatmentType'
import type { PagedResult } from '../../types/common'

export function TreatmentTypeListPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<PagedResult<TreatmentTypeDto> | null>(null)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 10

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      const result = await treatmentTypeApi.getPaged(page, pageSize)
      setData(result)
    } catch {
      // handle silently
    } finally {
      setLoading(false)
    }
  }, [page])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  const getIntervalLabel = (days: number): string => {
    const map: Record<number, string> = {
      7: '1 week', 14: '2 weeks', 21: '3 weeks', 28: '4 weeks',
      90: '3 months', 180: '6 months', 365: '1 year',
    }
    return map[days] || `${days} days`
  }

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center mb-3 gap-2">
        <h4 className="fw-bold mb-0">Treatment Types</h4>
        <Button variant="primary" size="sm" onClick={() => navigate('/treatment-types/new')}>
          + Add Treatment Type
        </Button>
      </div>

      <Card className="border-0 shadow-sm d-flex flex-column" style={{ minHeight: 'calc(100vh - 180px)' }}>
        <Card.Body className="d-flex flex-column">
          {loading ? (
            <div className="text-center py-4">
              <Spinner animation="border" variant="primary" size="sm" />
            </div>
          ) : !data || data.items.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No treatment types found.</p>
          ) : (
            <>
              <div className="table-responsive flex-grow-1">
                <Table hover className="mb-0 align-middle">
                  <thead className="table-light">
                    <tr>
                      <th>Name</th>
                      <th>Follow-Up Interval</th>
                      <th className="d-none d-md-table-cell">Notes</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((tt) => (
                      <tr key={tt.id}>
                        <td className="fw-medium">{tt.name}</td>
                        <td>
                          <Badge bg="info" className="text-white">
                            {getIntervalLabel(tt.followUpIntervalDays)}
                          </Badge>
                        </td>
                        <td className="d-none d-md-table-cell text-muted">{tt.notes || '—'}</td>
                        <td>
                          <Button
                            variant="outline-primary"
                            size="sm"
                            onClick={() => navigate(`/treatment-types/${tt.id}/edit`)}
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
                <div className="d-flex justify-content-between align-items-center mt-auto pt-3 border-top">
                  <small className="text-muted">
                    Showing {data.items.length} of {data.totalCount}
                  </small>
                  <Pagination
                    currentPage={page}
                    totalPages={data.totalPages}
                    totalCount={data.totalCount}
                    pageSize={data.pageSize}
                    onPageChange={setPage}
                  />
                </div>
              )}
            </>
          )}
        </Card.Body>
      </Card>
    </div>
  )
}


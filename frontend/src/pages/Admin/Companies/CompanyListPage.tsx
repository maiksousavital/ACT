import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Table, Button, Card, Spinner } from 'react-bootstrap'
import { companyApi } from '../../../api/companyApi'
import { Pagination } from '../../../components/Table/Pagination'
import type { CompanyDto } from '../../../types/company'
import type { PagedResult } from '../../../types/common'

export function CompanyListPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<PagedResult<CompanyDto> | null>(null)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      setData(await companyApi.getPaged(page, 10))
    } catch { /* */ } finally {
      setLoading(false)
    }
  }, [page])

  useEffect(() => { fetchData() }, [fetchData])

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center mb-3 gap-2">
        <h4 className="fw-bold mb-0">Companies</h4>
        <Button variant="primary" size="sm" onClick={() => navigate('/admin/companies/new')}>
          + Add Company
        </Button>
      </div>

      <Card className="border-0 shadow-sm">
        <Card.Body>
          {loading ? (
            <div className="text-center py-4"><Spinner animation="border" variant="primary" size="sm" /></div>
          ) : !data || data.items.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No companies found.</p>
          ) : (
            <>
              <div className="table-responsive">
                <Table hover className="mb-0 align-middle">
                  <thead className="table-light">
                    <tr>
                      <th>Name</th>
                      <th className="d-none d-md-table-cell">Contact Email</th>
                      <th className="d-none d-md-table-cell">Phone</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((c) => (
                      <tr key={c.id}>
                        <td className="fw-medium">{c.name}</td>
                        <td className="d-none d-md-table-cell">{c.contactEmail || '—'}</td>
                        <td className="d-none d-md-table-cell">{c.phone || '—'}</td>
                        <td>
                          <Button variant="outline-primary" size="sm" onClick={() => navigate(`/admin/companies/${c.id}/edit`)}>
                            Edit
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
              <div className="d-flex justify-content-between align-items-center mt-3">
                <small className="text-muted">Showing {data.items.length} of {data.totalCount}</small>
                <Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} />
              </div>
            </>
          )}
        </Card.Body>
      </Card>
    </div>
  )
}


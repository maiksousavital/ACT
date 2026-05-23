import { useEffect, useState, useCallback } from 'react'
import { Table, Card, Spinner, Badge, Form } from 'react-bootstrap'
import { auditApi } from '../../../api/auditApi'
import { companyApi } from '../../../api/companyApi'
import { Pagination } from '../../../components/Table/Pagination'
import { useAuth } from '../../../contexts/AuthContext'
import type { LoginHistoryDto } from '../../../types/audit'
import type { PagedResult } from '../../../types/common'
import type { CompanyDto } from '../../../types/company'

export function LoginHistoryPage() {
  const { isSuperAdmin } = useAuth()
  const [data, setData] = useState<PagedResult<LoginHistoryDto> | null>(null)
  const [companies, setCompanies] = useState<CompanyDto[]>([])
  const [companyId, setCompanyId] = useState<number | undefined>(undefined)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (isSuperAdmin) {
      companyApi.getPaged(1, 100).then((r) => setCompanies(r.items)).catch(() => {})
    }
  }, [isSuperAdmin])

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      setData(await auditApi.getLoginHistory(page, 20, companyId))
    } catch { /* */ } finally {
      setLoading(false)
    }
  }, [page, companyId])

  useEffect(() => { fetchData() }, [fetchData])

  return (
    <div>
      <h4 className="fw-bold mb-3">Login History</h4>

      {isSuperAdmin && companies.length > 0 && (
        <Form.Select
          size="sm"
          className="mb-3"
          style={{ maxWidth: '300px' }}
          value={companyId ?? ''}
          onChange={(e) => { setCompanyId(e.target.value ? Number(e.target.value) : undefined); setPage(1) }}
        >
          <option value="">All Companies</option>
          {companies.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </Form.Select>
      )}

      <Card className="border-0 shadow-sm">
        <Card.Body>
          {loading ? (
            <div className="text-center py-4"><Spinner animation="border" variant="primary" size="sm" /></div>
          ) : !data || data.items.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No login history found.</p>
          ) : (
            <>
              <div className="table-responsive">
                <Table hover size="sm" className="mb-0 align-middle">
                  <thead className="table-light">
                    <tr>
                      <th>Timestamp</th>
                      <th>Email</th>
                      <th>Status</th>
                      <th className="d-none d-md-table-cell">IP Address</th>
                      <th className="d-none d-lg-table-cell">Failure Reason</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((entry) => (
                      <tr key={entry.id}>
                        <td className="small">{new Date(entry.timestamp).toLocaleString()}</td>
                        <td className="small">{entry.email}</td>
                        <td>
                          <Badge bg={entry.success ? 'success' : 'danger'} className="small">
                            {entry.success ? 'Success' : 'Failed'}
                          </Badge>
                        </td>
                        <td className="d-none d-md-table-cell small">{entry.ipAddress || '—'}</td>
                        <td className="d-none d-lg-table-cell small text-muted">{entry.failureReason || '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
              <div className="d-flex justify-content-between align-items-center mt-3">
                <small className="text-muted">Page {data.page} of {data.totalPages}</small>
                <Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} />
              </div>
            </>
          )}
        </Card.Body>
      </Card>
    </div>
  )
}


import { useEffect, useState, useCallback } from 'react'
import { Table, Card, Spinner, Form } from 'react-bootstrap'
import { auditApi } from '../../../api/auditApi'
import { companyApi } from '../../../api/companyApi'
import { Pagination } from '../../../components/Table/Pagination'
import { useAuth } from '../../../contexts/AuthContext'
import type { AuditLogDto } from '../../../types/audit'
import type { PagedResult } from '../../../types/common'
import type { CompanyDto } from '../../../types/company'

export function AuditLogPage() {
  const { isSuperAdmin } = useAuth()
  const [data, setData] = useState<PagedResult<AuditLogDto> | null>(null)
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
      setData(await auditApi.getLogs(page, 20, companyId))
    } catch { /* */ } finally {
      setLoading(false)
    }
  }, [page, companyId])

  useEffect(() => { fetchData() }, [fetchData])

  return (
    <div>
      <h4 className="fw-bold mb-3">Audit Logs</h4>

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

      <Card className="border-0 shadow-sm d-flex flex-column" style={{ minHeight: 'calc(100vh - 220px)' }}>
        <Card.Body className="d-flex flex-column">
          {loading ? (
            <div className="text-center py-4"><Spinner animation="border" variant="primary" size="sm" /></div>
          ) : !data || data.items.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No audit logs found.</p>
          ) : (
            <>
              <div className="table-responsive flex-grow-1">
                <Table hover size="sm" className="mb-0 align-middle">
                  <thead className="table-light">
                    <tr>
                      <th>Timestamp</th>
                      <th>User</th>
                      <th className="d-none d-md-table-cell">Action</th>
                      <th className="d-none d-lg-table-cell">Entity</th>
                      <th className="d-none d-lg-table-cell">Details</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((log) => (
                      <tr key={log.id}>
                        <td className="small">{new Date(log.timestamp).toLocaleString()}</td>
                        <td className="small">{log.userEmail}</td>
                        <td className="d-none d-md-table-cell small">{log.action}</td>
                        <td className="d-none d-lg-table-cell small">{log.entityType} #{log.entityId}</td>
                        <td className="d-none d-lg-table-cell small text-muted text-truncate" style={{ maxWidth: '200px' }}>{log.details || '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
              <div className="d-flex justify-content-between align-items-center mt-auto pt-3 border-top">
                <small className="text-muted">Page {data.page} of {data.totalPages}</small>
                <Pagination currentPage={page} totalPages={data.totalPages} totalCount={data.totalCount} pageSize={data.pageSize} onPageChange={setPage} />
              </div>
            </>
          )}
        </Card.Body>
      </Card>
    </div>
  )
}


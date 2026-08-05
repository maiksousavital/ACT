import { useCallback, useEffect, useState } from 'react'
import { Dropdown, Modal, Form, Table, Spinner, InputGroup } from 'react-bootstrap'
import { auditApi } from '../../api/auditApi'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
import { Pagination } from '../Table/Pagination'
import type { AuditLogDto } from '../../types/audit'
import type { PagedResult } from '../../types/common'

type DatePreset = 'all' | 'today' | 'yesterday' | 'thisWeek' | 'lastWeek' | 'thisMonth' | 'lastMonth' | 'thisYear' | 'lastYear' | 'custom'

function startOfDay(d: Date) { const x = new Date(d); x.setHours(0, 0, 0, 0); return x }
function endOfDay(d: Date) { const x = new Date(d); x.setHours(23, 59, 59, 999); return x }
function startOfWeek(d: Date) { const x = startOfDay(d); x.setDate(x.getDate() - x.getDay()); return x }
function endOfWeek(d: Date) { const x = startOfWeek(d); x.setDate(x.getDate() + 6); return endOfDay(x) }
function startOfMonth(d: Date) { return new Date(d.getFullYear(), d.getMonth(), 1) }
function endOfMonth(d: Date) { return endOfDay(new Date(d.getFullYear(), d.getMonth() + 1, 0)) }
function startOfYear(d: Date) { return new Date(d.getFullYear(), 0, 1) }
function endOfYear(d: Date) { return endOfDay(new Date(d.getFullYear(), 11, 31)) }

function getDateRange(preset: DatePreset, customFrom: string, customTo: string): { from?: string; to?: string } {
  const now = new Date()
  switch (preset) {
    case 'today':
      return { from: startOfDay(now).toISOString(), to: endOfDay(now).toISOString() }
    case 'yesterday': {
      const y = new Date(now); y.setDate(y.getDate() - 1)
      return { from: startOfDay(y).toISOString(), to: endOfDay(y).toISOString() }
    }
    case 'thisWeek':
      return { from: startOfWeek(now).toISOString(), to: endOfWeek(now).toISOString() }
    case 'lastWeek': {
      const lw = new Date(now); lw.setDate(lw.getDate() - 7)
      return { from: startOfWeek(lw).toISOString(), to: endOfWeek(lw).toISOString() }
    }
    case 'thisMonth':
      return { from: startOfMonth(now).toISOString(), to: endOfMonth(now).toISOString() }
    case 'lastMonth': {
      const lm = new Date(now.getFullYear(), now.getMonth() - 1, 1)
      return { from: startOfMonth(lm).toISOString(), to: endOfMonth(lm).toISOString() }
    }
    case 'thisYear':
      return { from: startOfYear(now).toISOString(), to: endOfYear(now).toISOString() }
    case 'lastYear': {
      const ly = new Date(now.getFullYear() - 1, 0, 1)
      return { from: startOfYear(ly).toISOString(), to: endOfYear(ly).toISOString() }
    }
    case 'custom':
      return {
        from: customFrom ? startOfDay(new Date(customFrom)).toISOString() : undefined,
        to: customTo ? endOfDay(new Date(customTo)).toISOString() : undefined,
      }
    default:
      return {}
  }
}

interface EntityAuditLogButtonProps {
  entityType: string
  entityId: number
}

export function EntityAuditLogButton({ entityType, entityId }: EntityAuditLogButtonProps) {
  const [show, setShow] = useState(false)
  const [data, setData] = useState<PagedResult<AuditLogDto> | null>(null)
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [preset, setPreset] = useState<DatePreset>('all')
  const [customFrom, setCustomFrom] = useState('')
  const [customTo, setCustomTo] = useState('')

  const debouncedSearch = useDebouncedValue(search, 300)

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      const { from, to } = getDateRange(preset, customFrom, customTo)
      const result = await auditApi.getForEntity(entityType, entityId, {
        page, pageSize: 20, search: debouncedSearch || undefined, from, to,
      })
      setData(result)
    } catch {
      setData(null)
    } finally {
      setLoading(false)
    }
  }, [entityType, entityId, page, debouncedSearch, preset, customFrom, customTo])

  useEffect(() => {
    if (show) fetchData()
  }, [show, fetchData])

  return (
    <>
      <Dropdown align="end">
        <Dropdown.Toggle variant="outline-secondary" size="sm" id={`audit-menu-${entityType}-${entityId}`}>
          ⋮
        </Dropdown.Toggle>
        <Dropdown.Menu>
          <Dropdown.Item onClick={() => setShow(true)}>View Change History</Dropdown.Item>
        </Dropdown.Menu>
      </Dropdown>

      <Modal show={show} onHide={() => setShow(false)} size="lg" centered>
        <Modal.Header closeButton>
          <Modal.Title>Change History</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <div className="d-flex flex-column flex-sm-row gap-2 mb-3">
            <InputGroup size="sm" style={{ maxWidth: '260px' }}>
              <InputGroup.Text>Search</InputGroup.Text>
              <Form.Control value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} placeholder="Field, value, user..." />
            </InputGroup>
            <Form.Select size="sm" style={{ maxWidth: '180px' }} value={preset} onChange={(e) => { setPreset(e.target.value as DatePreset); setPage(1) }}>
              <option value="all">All time</option>
              <option value="today">Today</option>
              <option value="yesterday">Yesterday</option>
              <option value="thisWeek">This Week</option>
              <option value="lastWeek">Last Week</option>
              <option value="thisMonth">This Month</option>
              <option value="lastMonth">Last Month</option>
              <option value="thisYear">This Year</option>
              <option value="lastYear">Last Year</option>
              <option value="custom">Custom range...</option>
            </Form.Select>
            {preset === 'custom' && (
              <>
                <Form.Control size="sm" type="date" style={{ maxWidth: '160px' }} value={customFrom} onChange={(e) => { setCustomFrom(e.target.value); setPage(1) }} />
                <Form.Control size="sm" type="date" style={{ maxWidth: '160px' }} value={customTo} onChange={(e) => { setCustomTo(e.target.value); setPage(1) }} />
              </>
            )}
          </div>

          {loading ? (
            <div className="text-center py-4"><Spinner animation="border" variant="primary" size="sm" /></div>
          ) : !data || data.items.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No change history found.</p>
          ) : (
            <>
              <div className="table-responsive">
                <Table hover size="sm" className="mb-0 align-middle">
                  <thead className="table-light">
                    <tr>
                      <th>Date/Time</th>
                      <th>User</th>
                      <th>Field</th>
                      <th>From</th>
                      <th>To</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((log) => (
                      <tr key={log.id}>
                        <td className="small">{new Date(log.timestamp).toLocaleString()}</td>
                        <td className="small">{log.userEmail}</td>
                        <td className="small">{log.fieldName ?? log.action}</td>
                        <td className="small text-muted">{log.fieldName ? (log.oldValue || '—') : '—'}</td>
                        <td className="small">{log.fieldName ? (log.newValue || '—') : '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
              <div className="d-flex justify-content-between align-items-center mt-3 pt-2 border-top">
                <small className="text-muted">Page {data.page} of {data.totalPages}</small>
                <Pagination currentPage={page} totalPages={data.totalPages} totalCount={data.totalCount} pageSize={data.pageSize} onPageChange={setPage} />
              </div>
            </>
          )}
        </Modal.Body>
      </Modal>
    </>
  )
}

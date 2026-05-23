import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Table, Button, Card, Spinner, Modal } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { companyApi } from '../../../api/companyApi'
import { Pagination } from '../../../components/Table/Pagination'
import type { CompanyDto } from '../../../types/company'
import type { PagedResult } from '../../../types/common'

export function CompanyListPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<PagedResult<CompanyDto> | null>(null)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [deleteTarget, setDeleteTarget] = useState<CompanyDto | null>(null)

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      setData(await companyApi.getPaged(page, 10))
    } catch { /* */ } finally {
      setLoading(false)
    }
  }, [page])

  useEffect(() => { fetchData() }, [fetchData])

  const handleDelete = async () => {
    if (!deleteTarget) return
    try {
      await companyApi.delete(deleteTarget.id)
      toast.success(`"${deleteTarget.name}" has been deleted`)
      setDeleteTarget(null)
      await fetchData()
    } catch {
      toast.error('Failed to delete company')
    }
  }

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center mb-3 gap-2">
        <h4 className="fw-bold mb-0">Companies</h4>
        <Button variant="primary" size="sm" onClick={() => navigate('/admin/companies/new')}>
          + Add Company
        </Button>
      </div>

      <Card className="border-0 shadow-sm d-flex flex-column" style={{ minHeight: 'calc(100vh - 180px)' }}>
        <Card.Body className="d-flex flex-column">
          {loading ? (
            <div className="text-center py-4"><Spinner animation="border" variant="primary" size="sm" /></div>
          ) : !data || data.items.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No companies found.</p>
          ) : (
            <>
              <div className="table-responsive flex-grow-1">
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
                          <Button variant="outline-primary" size="sm" className="me-1" onClick={() => navigate(`/admin/companies/${c.id}/edit`)}>
                            Edit
                          </Button>
                          <Button variant="outline-danger" size="sm" onClick={() => setDeleteTarget(c)}>
                            Delete
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
              <div className="d-flex justify-content-between align-items-center mt-auto pt-3 border-top">
                <small className="text-muted">Showing {data.items.length} of {data.totalCount}</small>
                <Pagination currentPage={page} totalPages={data.totalPages} totalCount={data.totalCount} pageSize={data.pageSize} onPageChange={setPage} />
              </div>
            </>
          )}
        </Card.Body>
      </Card>

      {/* Delete Confirmation Modal */}
      <Modal show={!!deleteTarget} onHide={() => setDeleteTarget(null)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Delete Company</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to delete <strong>{deleteTarget?.name}</strong>? Their users will no longer be able to log in.
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={() => setDeleteTarget(null)}>Cancel</Button>
          <Button variant="danger" onClick={handleDelete}>Delete</Button>
        </Modal.Footer>
      </Modal>
    </div>
  )
}


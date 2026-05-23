import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Table, Button, Card, Spinner, Badge, Form, Modal } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { userApi } from '../../../api/userApi'
import { companyApi } from '../../../api/companyApi'
import type { UserDto } from '../../../types/user'
import type { CompanyDto } from '../../../types/company'
import { useAuth } from '../../../contexts/AuthContext'

export function UserListPage() {
  const { isSuperAdmin, user } = useAuth()
  const navigate = useNavigate()
  const [users, setUsers] = useState<UserDto[]>([])
  const [companies, setCompanies] = useState<CompanyDto[]>([])
  const [selectedCompanyId, setSelectedCompanyId] = useState<number | undefined>(
    user?.companyId ?? undefined
  )
  const [loading, setLoading] = useState(true)
  const [deleteId, setDeleteId] = useState<number | null>(null)

  const fetchUsers = useCallback(async () => {
    setLoading(true)
    try {
      const result = await userApi.getByCompany(selectedCompanyId)
      setUsers(result)
    } catch { /* */ } finally {
      setLoading(false)
    }
  }, [selectedCompanyId])

  useEffect(() => {
    if (isSuperAdmin) {
      companyApi.getPaged(1, 100).then((res) => setCompanies(res.items)).catch(() => {})
    }
  }, [isSuperAdmin])

  useEffect(() => { fetchUsers() }, [fetchUsers])

  const handleDelete = async () => {
    if (!deleteId) return
    try {
      await userApi.delete(deleteId)
      toast.success('User deleted')
      setDeleteId(null)
      await fetchUsers()
    } catch {
      toast.error('Failed to delete user')
    }
  }

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center mb-3 gap-2">
        <h4 className="fw-bold mb-0">Users</h4>
        <Button variant="primary" size="sm" onClick={() => navigate('/admin/users/new')}>
          + Add User
        </Button>
      </div>

      {isSuperAdmin && companies.length > 0 && (
        <Form.Select
          size="sm"
          className="mb-3"
          style={{ maxWidth: '300px' }}
          value={selectedCompanyId ?? ''}
          onChange={(e) => setSelectedCompanyId(e.target.value ? Number(e.target.value) : undefined)}
        >
          <option value="">All Companies</option>
          {companies.map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </Form.Select>
      )}

      <Card className="border-0 shadow-sm">
        <Card.Body>
          {loading ? (
            <div className="text-center py-4"><Spinner animation="border" variant="primary" size="sm" /></div>
          ) : users.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No users found.</p>
          ) : (
            <div className="table-responsive">
              <Table hover className="mb-0 align-middle">
                <thead className="table-light">
                  <tr>
                    <th>Email</th>
                    <th>Role</th>
                    <th className="d-none d-md-table-cell">Created</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((u) => (
                    <tr key={u.id}>
                      <td className="fw-medium">{u.email}</td>
                      <td><Badge bg="info" className="text-capitalize">{u.role}</Badge></td>
                      <td className="d-none d-md-table-cell">{new Date(u.createdAt).toLocaleDateString()}</td>
                      <td>
                        <Badge bg={u.isActive ? 'success' : 'secondary'}>
                          {u.isActive ? 'Active' : 'Inactive'}
                        </Badge>
                      </td>
                      <td>
                        <Button variant="outline-primary" size="sm" className="me-1" onClick={() => navigate(`/admin/users/${u.id}/edit`)}>
                          Edit
                        </Button>
                        {u.isActive && (
                          <Button variant="outline-danger" size="sm" onClick={() => setDeleteId(u.id)}>
                            Delete
                          </Button>
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

      {/* Delete Confirmation Modal */}
      <Modal show={!!deleteId} onHide={() => setDeleteId(null)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Delete User</Modal.Title>
        </Modal.Header>
        <Modal.Body>Are you sure you want to delete this user? They will no longer be able to log in.</Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={() => setDeleteId(null)}>Cancel</Button>
          <Button variant="danger" onClick={handleDelete}>Delete</Button>
        </Modal.Footer>
      </Modal>
    </div>
  )
}

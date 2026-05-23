import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Table, Button, Card, Spinner, Form, InputGroup, Badge } from 'react-bootstrap'
import { clientApi } from '../../api/clientApi'
import { Pagination } from '../../components/Table/Pagination'
import type { ClientDto } from '../../types/client'
import type { PagedResult } from '../../types/common'

export function ClientListPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<PagedResult<ClientDto> | null>(null)
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const pageSize = 10

  const fetchClients = useCallback(async () => {
    setLoading(true)
    try {
      const result = await clientApi.getPaged(page, pageSize)
      setData(result)
    } catch {
      // handle error silently
    } finally {
      setLoading(false)
    }
  }, [page])

  useEffect(() => {
    fetchClients()
  }, [fetchClients])

  const filteredItems = data?.items.filter((c) => {
    const name = `${c.firstName} ${c.lastName}`.toLowerCase()
    return name.includes(search.toLowerCase())
  }) ?? []

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center mb-3 gap-2">
        <h4 className="fw-bold mb-0">Clients</h4>
        <Button variant="primary" size="sm" onClick={() => navigate('/clients/new')}>
          + Add Client
        </Button>
      </div>

      <Card className="border-0 shadow-sm d-flex flex-column" style={{ minHeight: 'calc(100vh - 180px)' }}>
        <Card.Body className="d-flex flex-column">
          <InputGroup className="mb-3" size="sm">
            <Form.Control
              placeholder="Search by name..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </InputGroup>

          {loading ? (
            <div className="text-center py-4">
              <Spinner animation="border" variant="primary" size="sm" />
            </div>
          ) : filteredItems.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No clients found.</p>
          ) : (
            <>
              <div className="table-responsive flex-grow-1">
                <Table hover className="mb-0 align-middle">
                  <thead className="table-light">
                    <tr>
                      <th>Name</th>
                      <th className="d-none d-md-table-cell">Phone</th>
                      <th className="d-none d-md-table-cell">Email</th>
                      <th>Status</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredItems.map((client) => (
                      <tr key={client.id}>
                        <td>
                          <span
                            className="text-primary fw-medium"
                            role="button"
                            onClick={() => navigate(`/clients/${client.id}`)}
                          >
                            {client.firstName} {client.lastName}
                          </span>
                        </td>
                        <td className="d-none d-md-table-cell">{client.phone || '—'}</td>
                        <td className="d-none d-md-table-cell">{client.email || '—'}</td>
                        <td>
                          <Badge bg={client.isArchived ? 'secondary' : 'success'}>
                            {client.isArchived ? 'Archived' : 'Active'}
                          </Badge>
                        </td>
                        <td>
                          <Button
                            variant="outline-primary"
                            size="sm"
                            className="me-1"
                            onClick={() => navigate(`/clients/${client.id}/edit`)}
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
                    Showing {filteredItems.length} of {data.totalCount} clients
                  </small>
                  <Pagination
                    currentPage={page}
                    totalPages={data.totalPages}
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


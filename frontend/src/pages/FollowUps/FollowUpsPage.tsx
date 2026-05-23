import { useEffect, useState, useCallback } from 'react'
import { Table, Card, Spinner, Badge, Button, Modal, Form } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { followUpApi } from '../../api/treatmentApi'
import type { TreatmentDto } from '../../types/treatment'

export function FollowUpsPage() {
  const [todayItems, setTodayItems] = useState<TreatmentDto[]>([])
  const [overdueItems, setOverdueItems] = useState<TreatmentDto[]>([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [followUpNotes, setFollowUpNotes] = useState('')
  const [completing, setCompleting] = useState(false)

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      const [today, due] = await Promise.all([
        followUpApi.getToday(),
        followUpApi.getDue(),
      ])
      setTodayItems(today)
      setOverdueItems(due)
    } catch {
      // silently fail
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  const handleComplete = (id: number) => {
    setSelectedId(id)
    setFollowUpNotes('')
    setShowModal(true)
  }

  const confirmComplete = async () => {
    if (!selectedId) return
    setCompleting(true)
    try {
      await followUpApi.complete(selectedId, { followUpNotes: followUpNotes || undefined })
      toast.success('Follow-up completed')
      setShowModal(false)
      await fetchData()
    } catch {
      toast.error('Failed to complete follow-up')
    } finally {
      setCompleting(false)
    }
  }

  const renderTable = (items: TreatmentDto[], variant: 'info' | 'danger') => (
    <div className="table-responsive">
      <Table hover className="mb-0 align-middle">
        <thead className="table-light">
          <tr>
            <th>Client</th>
            <th className="d-none d-md-table-cell">Phone</th>
            <th className="d-none d-sm-table-cell">Treatment Type</th>
            <th>Due Date</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id}>
              <td className="fw-medium">{item.clientName}</td>
              <td className="d-none d-md-table-cell">{item.phone || '—'}</td>
              <td className="d-none d-sm-table-cell">{item.treatmentTypeName}</td>
              <td>
                <span className={variant === 'danger' ? 'text-danger' : ''}>
                  {new Date(item.nextFollowUpDate).toLocaleDateString()}
                </span>
              </td>
              <td>
                <Button variant="success" size="sm" onClick={() => handleComplete(item.id)}>
                  Complete
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  )

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    )
  }

  return (
    <div>
      <h4 className="fw-bold mb-3">Follow-Ups</h4>

      {/* Today */}
      <Card className="border-0 shadow-sm mb-4">
        <Card.Header className="bg-white border-bottom d-flex justify-content-between align-items-center">
          <h6 className="mb-0 fw-semibold">Due Today</h6>
          <Badge bg="info">{todayItems.length}</Badge>
        </Card.Header>
        <Card.Body className="p-0">
          {todayItems.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No follow-ups due today.</p>
          ) : (
            renderTable(todayItems, 'info')
          )}
        </Card.Body>
      </Card>

      {/* Overdue */}
      <Card className="border-0 shadow-sm">
        <Card.Header className="bg-white border-bottom d-flex justify-content-between align-items-center">
          <h6 className="mb-0 fw-semibold text-danger">Overdue</h6>
          <Badge bg="danger">{overdueItems.length}</Badge>
        </Card.Header>
        <Card.Body className="p-0">
          {overdueItems.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No overdue follow-ups. 🎉</p>
          ) : (
            renderTable(overdueItems, 'danger')
          )}
        </Card.Body>
      </Card>

      {/* Complete Modal */}
      <Modal show={showModal} onHide={() => setShowModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Complete Follow-Up</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form.Group>
            <Form.Label>Notes (optional)</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              placeholder="Any notes about this follow-up..."
              value={followUpNotes}
              onChange={(e) => setFollowUpNotes(e.target.value)}
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={() => setShowModal(false)}>
            Cancel
          </Button>
          <Button variant="success" onClick={confirmComplete} disabled={completing}>
            {completing ? <Spinner animation="border" size="sm" /> : 'Mark as Complete'}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  )
}


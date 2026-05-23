import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card, Row, Col, Button, Table, Spinner, Badge } from 'react-bootstrap'
import { clientApi } from '../../api/clientApi'
import { followUpApi } from '../../api/followUpApi'
import type { TreatmentDto } from '../../types/treatment'
import styles from './DashboardPage.module.css'

export function DashboardPage() {
  const navigate = useNavigate()
  const [totalClients, setTotalClients] = useState<number>(0)
  const [todayFollowUps, setTodayFollowUps] = useState<TreatmentDto[]>([])
  const [overdueFollowUps, setOverdueFollowUps] = useState<TreatmentDto[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function fetchData() {
      try {
        const [clientsRes, todayRes, dueRes] = await Promise.all([
          clientApi.getPaged(1, 1),
          followUpApi.getToday(),
          followUpApi.getDue(),
        ])
        setTotalClients(clientsRes.totalCount)
        setTodayFollowUps(todayRes)
        setOverdueFollowUps(dueRes)
      } catch {
        // silently fail — cards will show 0
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [])

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    )
  }

  const stats = [
    { label: 'Total Clients', value: totalClients, color: 'bg-primary bg-opacity-10 text-primary', icon: '👥' },
    { label: "Today's Follow-Ups", value: todayFollowUps.length, color: 'bg-info bg-opacity-10 text-info', icon: '📅' },
    { label: 'Overdue', value: overdueFollowUps.length, color: 'bg-danger bg-opacity-10 text-danger', icon: '⚠️' },
  ]

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center mb-4 gap-2">
        <h4 className="fw-bold mb-0">Dashboard</h4>
        <div className="d-flex gap-2">
          <Button variant="primary" size="sm" onClick={() => navigate('/clients/new')}>
            + Add Client
          </Button>
          <Button variant="secondary" size="sm" onClick={() => navigate('/treatments/new')}>
            + Add Treatment
          </Button>
        </div>
      </div>

      {/* Stat Cards */}
      <Row className="g-3 mb-4">
        {stats.map((stat) => (
          <Col xs={12} sm={6} lg={4} key={stat.label}>
            <Card className={`border-0 shadow-sm ${styles.statCard}`}>
              <Card.Body className="d-flex align-items-center gap-3">
                <div className={`${styles.iconCircle} ${stat.color}`}>{stat.icon}</div>
                <div>
                  <div className="text-muted small">{stat.label}</div>
                  <div className="fw-bold fs-4">{stat.value}</div>
                </div>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>

      {/* Today's Follow-Ups Table */}
      <Card className="border-0 shadow-sm mb-4">
        <Card.Header className="bg-white border-bottom d-flex justify-content-between align-items-center">
          <h6 className="mb-0 fw-semibold">Today&apos;s Follow-Ups</h6>
          <Badge bg="info">{todayFollowUps.length}</Badge>
        </Card.Header>
        <Card.Body className="p-0">
          {todayFollowUps.length === 0 ? (
            <p className="text-muted text-center py-4 mb-0">No follow-ups scheduled for today.</p>
          ) : (
            <div className="table-responsive">
              <Table hover className="mb-0">
                <thead className="table-light">
                  <tr>
                    <th>Client</th>
                    <th className="d-none d-md-table-cell">Treatment Type</th>
                    <th>Due Date</th>
                  </tr>
                </thead>
                <tbody>
                  {todayFollowUps.slice(0, 5).map((fu) => (
                    <tr key={fu.id}>
                      <td>{fu.clientFirstName} {fu.clientLastName}</td>
                      <td className="d-none d-md-table-cell">{fu.treatmentTypeName}</td>
                      <td>{new Date(fu.nextFollowUpDate).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </div>
          )}
        </Card.Body>
      </Card>

      {/* Overdue Follow-Ups Table */}
      {overdueFollowUps.length > 0 && (
        <Card className="border-0 shadow-sm">
          <Card.Header className="bg-white border-bottom d-flex justify-content-between align-items-center">
            <h6 className="mb-0 fw-semibold text-danger">Overdue Follow-Ups</h6>
            <Badge bg="danger">{overdueFollowUps.length}</Badge>
          </Card.Header>
          <Card.Body className="p-0">
            <div className="table-responsive">
              <Table hover className="mb-0">
                <thead className="table-light">
                  <tr>
                    <th>Client</th>
                    <th className="d-none d-md-table-cell">Treatment Type</th>
                    <th>Due Date</th>
                  </tr>
                </thead>
                <tbody>
                  {overdueFollowUps.slice(0, 10).map((fu) => (
                    <tr key={fu.id}>
                      <td>{fu.clientFirstName} {fu.clientLastName}</td>
                      <td className="d-none d-md-table-cell">{fu.treatmentTypeName}</td>
                      <td className="text-danger">{new Date(fu.nextFollowUpDate).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </div>
          </Card.Body>
        </Card>
      )}
    </div>
  )
}


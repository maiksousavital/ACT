import { useEffect, useState } from 'react'
import { Form, Button, Card, Spinner, Alert, Row, Col } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { brandApi } from '../../api/brandApi'
import { useBrand } from '../../contexts/BrandContext'
import type { BrandSettingsDto } from '../../types/brand'
import styles from './BrandSettingsPage.module.css'

interface FormState {
  primaryColor: string
  secondaryColor: string
  accentColor: string
  theme: string
  logoUrl: string
}

const DEFAULTS: FormState = {
  primaryColor: '#6366F1',
  secondaryColor: '#06B6D4',
  accentColor: '#8B5CF6',
  theme: 'light',
  logoUrl: '',
}

export function BrandSettingsPage() {
  const { refresh } = useBrand()
  const [form, setForm] = useState<FormState>(DEFAULTS)
  const [existing, setExisting] = useState<BrandSettingsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function fetch() {
      try {
        const settings = await brandApi.get()
        if (settings) {
          setExisting(settings)
          setForm({
            primaryColor: settings.primaryColor || DEFAULTS.primaryColor,
            secondaryColor: settings.secondaryColor || DEFAULTS.secondaryColor,
            accentColor: settings.accentColor || DEFAULTS.accentColor,
            theme: settings.theme || DEFAULTS.theme,
            logoUrl: settings.logoUrl || '',
          })
        }
      } catch {
        // no brand settings yet — use defaults
      } finally {
        setLoading(false)
      }
    }
    fetch()
  }, [])

  const handleChange = (field: keyof FormState, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      const payload = {
        primaryColor: form.primaryColor,
        secondaryColor: form.secondaryColor,
        accentColor: form.accentColor,
        theme: form.theme,
        logoUrl: form.logoUrl || undefined,
      }

      if (existing) {
        await brandApi.update(payload)
      } else {
        await brandApi.create(payload)
      }
      await refresh()
      toast.success('Brand settings saved')
    } catch {
      setError('Failed to save settings. Please try again.')
    } finally {
      setSaving(false)
    }
  }

  const handleReset = () => {
    setForm(DEFAULTS)
  }

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    )
  }

  return (
    <div>
      <h4 className="fw-bold mb-3">Brand Settings</h4>

      {error && <Alert variant="danger">{error}</Alert>}

      <Row className="g-4">
        {/* Form */}
        <Col xs={12} lg={7}>
          <Card className="border-0 shadow-sm">
            <Card.Body className="p-3 p-md-4">
              <h6 className="fw-semibold mb-3">Colors</h6>

              <Row className="g-3 mb-4">
                <Col xs={12} sm={4}>
                  <Form.Group>
                    <Form.Label className="small text-muted">Primary Color</Form.Label>
                    <div className="d-flex align-items-center gap-2">
                      <Form.Control
                        type="color"
                        value={form.primaryColor}
                        onChange={(e) => handleChange('primaryColor', e.target.value)}
                        className={styles.colorPreview}
                      />
                      <Form.Control
                        type="text"
                        size="sm"
                        value={form.primaryColor}
                        onChange={(e) => handleChange('primaryColor', e.target.value)}
                      />
                    </div>
                  </Form.Group>
                </Col>
                <Col xs={12} sm={4}>
                  <Form.Group>
                    <Form.Label className="small text-muted">Secondary Color</Form.Label>
                    <div className="d-flex align-items-center gap-2">
                      <Form.Control
                        type="color"
                        value={form.secondaryColor}
                        onChange={(e) => handleChange('secondaryColor', e.target.value)}
                        className={styles.colorPreview}
                      />
                      <Form.Control
                        type="text"
                        size="sm"
                        value={form.secondaryColor}
                        onChange={(e) => handleChange('secondaryColor', e.target.value)}
                      />
                    </div>
                  </Form.Group>
                </Col>
                <Col xs={12} sm={4}>
                  <Form.Group>
                    <Form.Label className="small text-muted">Accent Color</Form.Label>
                    <div className="d-flex align-items-center gap-2">
                      <Form.Control
                        type="color"
                        value={form.accentColor}
                        onChange={(e) => handleChange('accentColor', e.target.value)}
                        className={styles.colorPreview}
                      />
                      <Form.Control
                        type="text"
                        size="sm"
                        value={form.accentColor}
                        onChange={(e) => handleChange('accentColor', e.target.value)}
                      />
                    </div>
                  </Form.Group>
                </Col>
              </Row>

              <h6 className="fw-semibold mb-3">Theme</h6>
              <Form.Group className="mb-4">
                <Form.Select
                  value={form.theme}
                  onChange={(e) => handleChange('theme', e.target.value)}
                >
                  <option value="light">Light</option>
                  <option value="dark">Dark</option>
                  <option value="custom">Custom</option>
                </Form.Select>
              </Form.Group>

              <h6 className="fw-semibold mb-3">Logo</h6>
              <Form.Group className="mb-4">
                <Form.Label className="small text-muted">Logo URL</Form.Label>
                <Form.Control
                  type="url"
                  placeholder="https://example.com/logo.png"
                  value={form.logoUrl}
                  onChange={(e) => handleChange('logoUrl', e.target.value)}
                />
                <Form.Text className="text-muted">
                  Enter the URL of your company logo. Recommended size: 200x60px.
                </Form.Text>
              </Form.Group>

              <div className="d-flex gap-2">
                <Button variant="primary" onClick={handleSave} disabled={saving}>
                  {saving ? <Spinner animation="border" size="sm" /> : 'Save Changes'}
                </Button>
                <Button variant="outline-secondary" onClick={handleReset}>
                  Reset to Defaults
                </Button>
              </div>
            </Card.Body>
          </Card>
        </Col>

        {/* Preview */}
        <Col xs={12} lg={5}>
          <Card className={`border-0 shadow-sm ${styles.previewCard}`}>
            <Card.Header className="bg-white border-bottom">
              <h6 className="mb-0 fw-semibold">Live Preview</h6>
            </Card.Header>
            <Card.Body>
              <div className="mb-3">
                <small className="text-muted d-block mb-1">Top Bar</small>
                <div
                  className={`${styles.previewBar} d-flex align-items-center px-3`}
                  style={{ backgroundColor: form.primaryColor }}
                >
                  {form.logoUrl ? (
                    <img src={form.logoUrl} alt="Logo" className="h-75" />
                  ) : (
                    <span className="text-white fw-bold">ACT</span>
                  )}
                </div>
              </div>

              <div className="mb-3">
                <small className="text-muted d-block mb-1">Buttons</small>
                <div className="d-flex gap-2 flex-wrap">
                  <button
                    className="btn btn-sm text-white"
                    style={{ backgroundColor: form.primaryColor }}
                  >
                    Primary
                  </button>
                  <button
                    className="btn btn-sm text-white"
                    style={{ backgroundColor: form.secondaryColor }}
                  >
                    Secondary
                  </button>
                  <button
                    className="btn btn-sm text-white"
                    style={{ backgroundColor: form.accentColor }}
                  >
                    Accent
                  </button>
                </div>
              </div>

              <div className="mb-3">
                <small className="text-muted d-block mb-1">Sidebar</small>
                <div
                  className={`${styles.previewBar} rounded`}
                  style={{ backgroundColor: '#1E293B' }}
                >
                  <div className="d-flex align-items-center h-100 px-3 gap-2">
                    <div className="rounded-1" style={{ width: 12, height: 12, backgroundColor: form.primaryColor }} />
                    <div className="rounded-1 bg-white bg-opacity-50" style={{ width: 60, height: 8 }} />
                  </div>
                </div>
              </div>

              <div>
                <small className="text-muted d-block mb-1">Color Palette</small>
                <div className="d-flex gap-2">
                  <div className={styles.colorPreview} style={{ backgroundColor: form.primaryColor }} />
                  <div className={styles.colorPreview} style={{ backgroundColor: form.secondaryColor }} />
                  <div className={styles.colorPreview} style={{ backgroundColor: form.accentColor }} />
                </div>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </div>
  )
}


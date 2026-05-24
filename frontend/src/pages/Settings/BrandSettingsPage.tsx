import { useEffect, useState } from 'react'
import { Form, Button, Card, Spinner, Alert, Row, Col } from 'react-bootstrap'
import toast from 'react-hot-toast'
import { brandApi } from '../../api/brandApi'
import { companyApi } from '../../api/companyApi'
import { useBrand, applyBrandToRoot } from '../../contexts/BrandContext'
import { useAuth } from '../../contexts/AuthContext'
import type { BrandSettingsDto } from '../../types/brand'
import type { CompanyDto } from '../../types/company'
import styles from './BrandSettingsPage.module.css'

interface FormState {
  primaryColor: string
  secondaryColor: string
  accentColor: string
  theme: string
  logoUrl: string
}

const THEME_PALETTES: Record<string, Omit<FormState, 'logoUrl' | 'theme'>> = {
  light: {
    primaryColor: '#6366F1',
    secondaryColor: '#06B6D4',
    accentColor: '#8B5CF6',
  },
  dark: {
    primaryColor: '#818CF8',
    secondaryColor: '#22D3EE',
    accentColor: '#A78BFA',
  },
  custom: {
    primaryColor: '#10B981',
    secondaryColor: '#F59E0B',
    accentColor: '#EF4444',
  },
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
  const { isSuperAdmin, user } = useAuth()
  const [form, setForm] = useState<FormState>(DEFAULTS)
  const [existing, setExisting] = useState<BrandSettingsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [companies, setCompanies] = useState<CompanyDto[]>([])
  const [selectedCompanyId, setSelectedCompanyId] = useState<number | undefined>(
    user?.companyId ?? undefined
  )

  // Ensure Admin users always have their companyId set
  useEffect(() => {
    if (!isSuperAdmin && user?.companyId && !selectedCompanyId) {
      setSelectedCompanyId(user.companyId)
    }
  }, [user?.companyId, isSuperAdmin, selectedCompanyId])

  useEffect(() => {
    if (isSuperAdmin) {
      companyApi.getPaged(1, 100).then((res) => setCompanies(res.items)).catch(() => {})
    }
  }, [isSuperAdmin])

  useEffect(() => {
    async function fetch() {
      // SuperAdmin must select a company first; Admin can load immediately
      if (isSuperAdmin && !selectedCompanyId) {
        setLoading(false)
        return
      }
      setLoading(true)
      try {
        const companyParam = isSuperAdmin ? selectedCompanyId : undefined
        const settings = await brandApi.get(companyParam)
        if (settings) {
          setExisting(settings)
          setForm({
            primaryColor: settings.primaryColor || DEFAULTS.primaryColor,
            secondaryColor: settings.secondaryColor || DEFAULTS.secondaryColor,
            accentColor: settings.accentColor || DEFAULTS.accentColor,
            sidebarColor: settings.sidebarColor || DEFAULTS.sidebarColor,
            backgroundColor: settings.backgroundColor || DEFAULTS.backgroundColor,
            theme: settings.theme || DEFAULTS.theme,
            logoUrl: settings.logoUrl || '',
          })
        } else {
          setExisting(null)
          setForm(DEFAULTS)
        }
      } catch {
        setExisting(null)
        setForm(DEFAULTS)
      } finally {
        setLoading(false)
      }
    }
    fetch()
  }, [selectedCompanyId, isSuperAdmin])

  const handleChange = (field: keyof FormState, value: string) => {
    if (field === 'theme') {
      const palette = THEME_PALETTES[value]
      if (palette) {
        setForm((prev) => ({
          ...prev,
          theme: value,
          primaryColor: palette.primaryColor,
          secondaryColor: palette.secondaryColor,
          accentColor: palette.accentColor,
          sidebarColor: palette.sidebarColor,
          backgroundColor: palette.backgroundColor,
        }))
        return
      }
    }
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  // Live preview — apply changes immediately as user edits
  useEffect(() => {
    applyBrandToRoot({
      primaryColor: form.primaryColor,
      secondaryColor: form.secondaryColor,
      accentColor: form.accentColor,
      sidebarColor: form.sidebarColor,
      backgroundColor: form.backgroundColor,
      theme: form.theme,
      logoUrl: form.logoUrl,
    } as BrandSettingsDto)
  }, [form])

  const handleSave = async () => {
    // SuperAdmin must select a company; Admin users don't need to — backend uses JWT
    const companyParam = isSuperAdmin ? selectedCompanyId : undefined
    if (isSuperAdmin && !companyParam) {
      setError('Please select a company first.')
      return
    }
    setSaving(true)
    setError(null)
    try {
      const payload = {
        primaryColor: form.primaryColor,
        secondaryColor: form.secondaryColor,
        accentColor: form.accentColor,
        sidebarColor: form.sidebarColor,
        backgroundColor: form.backgroundColor,
        theme: form.theme,
        logoUrl: form.logoUrl || undefined,
      }

      if (existing) {
        await brandApi.update(payload, companyParam)
      } else {
        await brandApi.create(payload, companyParam)
      }
      // Reload saved settings to confirm persistence
      const saved = await brandApi.get(companyParam)
      if (saved) setExisting(saved)
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

      {isSuperAdmin && companies.length > 0 && (
        <Form.Select
          size="sm"
          className="mb-3"
          style={{ maxWidth: '300px' }}
          value={selectedCompanyId ?? ''}
          onChange={(e) => setSelectedCompanyId(e.target.value ? Number(e.target.value) : undefined)}
        >
          <option value="">Select a company...</option>
          {companies.map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </Form.Select>
      )}

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

              <h6 className="fw-semibold mb-3">Layout Colors</h6>
              <Row className="g-3 mb-4">
                <Col xs={12} sm={6}>
                  <Form.Group>
                    <Form.Label className="small text-muted">Sidebar Color</Form.Label>
                    <div className="d-flex align-items-center gap-2">
                      <Form.Control
                        type="color"
                        value={form.sidebarColor || '#1E293B'}
                        onChange={(e) => handleChange('sidebarColor', e.target.value)}
                        className={styles.colorPreview}
                      />
                      <Form.Control
                        type="text"
                        size="sm"
                        value={form.sidebarColor || '#1E293B'}
                        onChange={(e) => handleChange('sidebarColor', e.target.value)}
                      />
                    </div>
                    <Form.Text className="text-muted">Text auto-adjusts to contrast</Form.Text>
                  </Form.Group>
                </Col>
                <Col xs={12} sm={6}>
                  <Form.Group>
                    <Form.Label className="small text-muted">Background Color</Form.Label>
                    <div className="d-flex align-items-center gap-2">
                      <Form.Control
                        type="color"
                        value={form.backgroundColor || '#F8FAFC'}
                        onChange={(e) => handleChange('backgroundColor', e.target.value)}
                        className={styles.colorPreview}
                      />
                      <Form.Control
                        type="text"
                        size="sm"
                        value={form.backgroundColor || '#F8FAFC'}
                        onChange={(e) => handleChange('backgroundColor', e.target.value)}
                      />
                    </div>
                    <Form.Text className="text-muted">Text auto-adjusts to contrast</Form.Text>
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
          <Card className={`border-0 shadow-sm ${styles.previewCard}`} data-bs-theme={form.theme === 'dark' ? 'dark' : 'light'}>
            <Card.Header className="border-bottom">
              <h6 className="mb-0 fw-semibold">Live Preview</h6>
            </Card.Header>
            <Card.Body>
              <div className="mb-3">
                <small className="text-muted d-block mb-1">Top Bar</small>
                <div
                  className={`${styles.previewBar} d-flex align-items-center px-3`}
                  style={{ backgroundColor: form.primaryColor || '#6366F1' }}
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
                    style={{ backgroundColor: form.primaryColor || '#6366F1' }}
                  >
                    Primary
                  </button>
                  <button
                    className="btn btn-sm text-white"
                    style={{ backgroundColor: form.secondaryColor || '#06B6D4' }}
                  >
                    Secondary
                  </button>
                  <button
                    className="btn btn-sm text-white"
                    style={{ backgroundColor: form.accentColor || '#8B5CF6' }}
                  >
                    Accent
                  </button>
                </div>
              </div>

              <div className="mb-3">
                <small className="text-muted d-block mb-1">Sidebar</small>
                <div
                  className={`${styles.previewBar} rounded`}
                  style={{ backgroundColor: form.sidebarColor || '#1E293B' }}
                >
                  <div className="d-flex align-items-center h-100 px-3 gap-2">
                    <div className="rounded-1" style={{ width: 12, height: 12, backgroundColor: form.primaryColor || '#6366F1' }} />
                    <div className="rounded-1" style={{ width: 60, height: 8, backgroundColor: 'currentColor', opacity: 0.5 }} />
                  </div>
                </div>
              </div>

              <div className="mb-3">
                <small className="text-muted d-block mb-1">Page Background</small>
                <div
                  className={`${styles.previewBar} rounded border`}
                  style={{ backgroundColor: form.backgroundColor || '#F8FAFC' }}
                >
                  <div className="d-flex align-items-center h-100 px-3">
                    <small style={{ color: (form.backgroundColor || '#F8FAFC') > '#888888' ? '#333' : '#eee' }}>Content area</small>
                  </div>
                </div>
              </div>

              <div>
                <small className="text-muted d-block mb-1">Color Palette</small>
                <div className="d-flex gap-3">
                  <div className="text-center">
                    <div className={styles.colorPreview} style={{ backgroundColor: form.primaryColor || '#6366F1' }} />
                    <small className="d-block mt-1" style={{ fontSize: '0.7rem' }}>Primary</small>
                  </div>
                  <div className="text-center">
                    <div className={styles.colorPreview} style={{ backgroundColor: form.secondaryColor || '#06B6D4' }} />
                    <small className="d-block mt-1" style={{ fontSize: '0.7rem' }}>Secondary</small>
                  </div>
                  <div className="text-center">
                    <div className={styles.colorPreview} style={{ backgroundColor: form.accentColor || '#8B5CF6' }} />
                    <small className="d-block mt-1" style={{ fontSize: '0.7rem' }}>Accent</small>
                  </div>
                  <div className="text-center">
                    <div className={styles.colorPreview} style={{ backgroundColor: form.sidebarColor || '#1E293B' }} />
                    <small className="d-block mt-1" style={{ fontSize: '0.7rem' }}>Sidebar</small>
                  </div>
                  <div className="text-center">
                    <div className={styles.colorPreview} style={{ backgroundColor: form.backgroundColor || '#F8FAFC', border: '1px solid #ddd' }} />
                    <small className="d-block mt-1" style={{ fontSize: '0.7rem' }}>Background</small>
                  </div>
                </div>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </div>
  )
}


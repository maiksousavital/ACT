import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from 'react'
import { brandApi } from '../api/brandApi'
import { useAuth } from './AuthContext'
import type { BrandSettingsDto } from '../types/brand'

interface BrandContextType {
  brand: BrandSettingsDto | null
  loading: boolean
  refresh: () => Promise<void>
}

const BrandContext = createContext<BrandContextType | undefined>(undefined)

const DEFAULT_COLORS = {
  primary: '#6366F1',
  secondary: '#06B6D4',
  accent: '#8B5CF6',
  sidebar: '#1E293B',
  background: '#F8FAFC',
}

function hexToRgb(hex: string): string {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
  if (!result) return '99, 102, 241'
  return `${parseInt(result[1], 16)}, ${parseInt(result[2], 16)}, ${parseInt(result[3], 16)}`
}

function getLuminance(hex: string): number {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
  if (!result) return 0.5
  const [r, g, b] = [parseInt(result[1], 16) / 255, parseInt(result[2], 16) / 255, parseInt(result[3], 16) / 255]
  const toLinear = (c: number) => c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4)
  return 0.2126 * toLinear(r) + 0.7152 * toLinear(g) + 0.0722 * toLinear(b)
}

function getContrastText(hex: string): string {
  return getLuminance(hex) > 0.4 ? '#1E293B' : '#FFFFFF'
}

function darken(hex: string, amount: number): string {
  const num = parseInt(hex.replace('#', ''), 16)
  const r = Math.max(0, ((num >> 16) & 0xff) - amount)
  const g = Math.max(0, ((num >> 8) & 0xff) - amount)
  const b = Math.max(0, (num & 0xff) - amount)
  return `rgb(${r}, ${g}, ${b})`
}

function lighten(hex: string, amount: number): string {
  const num = parseInt(hex.replace('#', ''), 16)
  const r = Math.min(255, ((num >> 16) & 0xff) + amount)
  const g = Math.min(255, ((num >> 8) & 0xff) + amount)
  const b = Math.min(255, (num & 0xff) + amount)
  return `rgb(${r}, ${g}, ${b})`
}

export function applyBrandToRoot(brand: BrandSettingsDto | null) {
  const root = document.documentElement
  const primary = brand?.primaryColor || DEFAULT_COLORS.primary
  const secondary = brand?.secondaryColor || DEFAULT_COLORS.secondary
  const accent = brand?.accentColor || DEFAULT_COLORS.accent
  const sidebarBg = brand?.sidebarColor || DEFAULT_COLORS.sidebar
  const pageBg = brand?.backgroundColor || DEFAULT_COLORS.background

  // Auto-contrast text colors
  const sidebarText = getContrastText(sidebarBg)
  const sidebarTextMuted = getLuminance(sidebarBg) > 0.4 ? 'rgba(0,0,0,0.6)' : 'rgba(255,255,255,0.7)'
  const pageText = getContrastText(pageBg)

  // Apply dark/light theme
  if (brand?.theme === 'dark') {
    root.setAttribute('data-bs-theme', 'dark')
  } else {
    root.setAttribute('data-bs-theme', 'light')
  }

  // Set CSS variables
  root.style.setProperty('--bs-primary', primary)
  root.style.setProperty('--bs-primary-rgb', hexToRgb(primary))
  root.style.setProperty('--bs-secondary', secondary)
  root.style.setProperty('--bs-secondary-rgb', hexToRgb(secondary))
  root.style.setProperty('--bs-info', accent)
  root.style.setProperty('--bs-info-rgb', hexToRgb(accent))
  root.style.setProperty('--act-sidebar-bg', sidebarBg)
  root.style.setProperty('--act-sidebar-text', sidebarText)
  root.style.setProperty('--act-sidebar-text-muted', sidebarTextMuted)
  root.style.setProperty('--act-page-bg', pageBg)
  root.style.setProperty('--act-page-text', pageText)

  // Inject dynamic stylesheet
  let styleEl = document.getElementById('act-brand-styles') as HTMLStyleElement | null
  if (!styleEl) {
    styleEl = document.createElement('style')
    styleEl.id = 'act-brand-styles'
    document.head.appendChild(styleEl)
  }

  styleEl.textContent = `
    :root {
      --bs-primary: ${primary};
      --bs-primary-rgb: ${hexToRgb(primary)};
      --bs-secondary: ${secondary};
      --bs-secondary-rgb: ${hexToRgb(secondary)};
      --bs-info: ${accent};
      --bs-info-rgb: ${hexToRgb(accent)};
      --act-sidebar-bg: ${sidebarBg};
      --act-sidebar-text: ${sidebarText};
      --act-sidebar-text-muted: ${sidebarTextMuted};
      --act-page-bg: ${pageBg};
      --act-page-text: ${pageText};
    }

    /* Page background & text */
    body {
      background-color: ${pageBg} !important;
      color: ${pageText} !important;
    }

    .card {
      color: ${pageText};
    }

    /* Sidebar */
    .bg-dark, [class*="sidebar"] nav {
      background-color: ${sidebarBg} !important;
    }

    .bg-dark h5, .bg-dark .fw-bold {
      color: ${sidebarText} !important;
    }

    /* Buttons */
    .btn-primary {
      --bs-btn-bg: ${primary};
      --bs-btn-border-color: ${primary};
      --bs-btn-hover-bg: ${darken(primary, 20)};
      --bs-btn-hover-border-color: ${darken(primary, 25)};
      --bs-btn-active-bg: ${darken(primary, 30)};
      --bs-btn-active-border-color: ${darken(primary, 35)};
      --bs-btn-disabled-bg: ${primary};
      --bs-btn-disabled-border-color: ${primary};
    }

    .btn-outline-primary {
      --bs-btn-color: ${primary};
      --bs-btn-border-color: ${primary};
      --bs-btn-hover-bg: ${primary};
      --bs-btn-hover-border-color: ${primary};
      --bs-btn-active-bg: ${primary};
      --bs-btn-active-border-color: ${primary};
    }

    .btn-secondary {
      --bs-btn-bg: ${secondary};
      --bs-btn-border-color: ${secondary};
      --bs-btn-hover-bg: ${darken(secondary, 20)};
      --bs-btn-hover-border-color: ${darken(secondary, 25)};
    }

    .btn-info {
      --bs-btn-bg: ${accent};
      --bs-btn-border-color: ${accent};
      --bs-btn-hover-bg: ${darken(accent, 20)};
      --bs-btn-hover-border-color: ${darken(accent, 25)};
    }

    /* Text utilities */
    .text-primary { color: ${primary} !important; }
    .bg-primary { background-color: ${primary} !important; }
    .border-primary { border-color: ${primary} !important; }

    .text-secondary { color: ${secondary} !important; }
    .bg-secondary { background-color: ${secondary} !important; }

    .text-info { color: ${accent} !important; }
    .bg-info { background-color: ${accent} !important; }

    .badge.bg-primary { background-color: ${primary} !important; }
    .badge.bg-info { background-color: ${accent} !important; }

    .spinner-border.text-primary { color: ${primary} !important; }

    .form-check-input:checked {
      background-color: ${primary};
      border-color: ${primary};
    }

    .form-control:focus, .form-select:focus {
      border-color: ${lighten(primary, 40)};
      box-shadow: 0 0 0 0.25rem rgba(${hexToRgb(primary)}, 0.25);
    }

    .nav-link.active, .nav-pills .nav-link.active {
      background-color: ${primary};
    }

    a { color: ${primary}; }
    a:hover { color: ${darken(primary, 20)}; }

    .pagination .page-item.active .page-link {
      background-color: ${primary};
      border-color: ${primary};
    }
  `
}

export function BrandProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated, user } = useAuth()
  const [brand, setBrand] = useState<BrandSettingsDto | null>(null)
  const [loading, setLoading] = useState(false)

  const refresh = useCallback(async () => {
    if (!isAuthenticated || !user?.companyId) {
      applyBrandToRoot(null)
      return
    }
    setLoading(true)
    try {
      const settings = await brandApi.get()
      setBrand(settings)
      applyBrandToRoot(settings)
    } catch {
      applyBrandToRoot(null)
    } finally {
      setLoading(false)
    }
  }, [isAuthenticated, user?.companyId])

  useEffect(() => {
    refresh()
  }, [refresh])

  return (
    <BrandContext.Provider value={{ brand, loading, refresh }}>
      {children}
    </BrandContext.Provider>
  )
}

export function useBrand() {
  const context = useContext(BrandContext)
  if (!context) throw new Error('useBrand must be used within BrandProvider')
  return context
}

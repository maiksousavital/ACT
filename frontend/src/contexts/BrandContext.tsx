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
}

function applyBrandToRoot(brand: BrandSettingsDto | null) {
  const root = document.documentElement
  if (brand?.primaryColor) {
    root.style.setProperty('--bs-primary', brand.primaryColor)
  } else {
    root.style.setProperty('--bs-primary', DEFAULT_COLORS.primary)
  }
  if (brand?.secondaryColor) {
    root.style.setProperty('--bs-secondary', brand.secondaryColor)
  } else {
    root.style.setProperty('--bs-secondary', DEFAULT_COLORS.secondary)
  }
  if (brand?.accentColor) {
    root.style.setProperty('--bs-info', brand.accentColor)
  } else {
    root.style.setProperty('--bs-info', DEFAULT_COLORS.accent)
  }
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


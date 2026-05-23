import { createContext, useContext, useReducer, useEffect, type ReactNode } from 'react'
import type { User } from '../types/auth'
import { getToken, setToken, removeToken, isTokenExpired, decodeToken } from '../utils/token'
import { authApi } from '../api/authApi'
import type { LoginRequest } from '../types/auth'

interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isLoading: boolean
}

type AuthAction =
  | { type: 'LOGIN_SUCCESS'; payload: { user: User; token: string } }
  | { type: 'LOGOUT' }
  | { type: 'LOADED' }

const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: true,
}

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case 'LOGIN_SUCCESS':
      return {
        user: action.payload.user,
        token: action.payload.token,
        isAuthenticated: true,
        isLoading: false,
      }
    case 'LOGOUT':
      return { ...initialState, isLoading: false }
    case 'LOADED':
      return { ...state, isLoading: false }
    default:
      return state
  }
}

interface AuthContextType extends AuthState {
  login: (data: LoginRequest) => Promise<void>
  logout: () => void
  isSuperAdmin: boolean
  isAdmin: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialState)

  useEffect(() => {
    const token = getToken()
    if (token && !isTokenExpired(token)) {
      const decoded = decodeToken(token)
      const user: User = {
        email: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || decoded.email || '',
        role: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role || '',
        companyId: decoded.CompanyId ? Number(decoded.CompanyId) : null,
      }
      dispatch({ type: 'LOGIN_SUCCESS', payload: { user, token } })
    } else {
      if (token) removeToken()
      dispatch({ type: 'LOADED' })
    }
  }, [])

  const login = async (data: LoginRequest) => {
    const response = await authApi.login(data)
    setToken(response.token)
    const decoded = decodeToken(response.token)
    const user: User = {
      email: response.email || decoded.email || '',
      role: response.role || decoded.role || '',
      companyId: response.companyId,
    }
    dispatch({ type: 'LOGIN_SUCCESS', payload: { user, token: response.token } })
  }

  const logout = () => {
    removeToken()
    dispatch({ type: 'LOGOUT' })
  }

  const isSuperAdmin = state.user?.role === 'SuperAdmin'
  const isAdmin = state.user?.role === 'Admin' || isSuperAdmin

  return (
    <AuthContext.Provider value={{ ...state, login, logout, isSuperAdmin, isAdmin }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within AuthProvider')
  return context
}


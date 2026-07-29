import { createContext, useContext, useReducer, useEffect, type ReactNode } from 'react'
import type { User } from '../types/auth'
import { authApi } from '../api/authApi'
import type { LoginRequest } from '../types/auth'

interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
}

type AuthAction =
  | { type: 'LOGIN_SUCCESS'; payload: { user: User } }
  | { type: 'LOGOUT' }
  | { type: 'LOADED' }

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: true,
}

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case 'LOGIN_SUCCESS':
      return {
        user: action.payload.user,
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
  logout: () => Promise<void>
  isSuperAdmin: boolean
  isAdmin: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialState)

  // The JWT lives in an httpOnly cookie invisible to JS (see E1), so there's nothing to decode
  // client-side on load — instead, ask the backend who the cookie belongs to. A 401 here just
  // means "not logged in", not an error.
  useEffect(() => {
    authApi
      .me()
      .then((response) => {
        const user: User = { email: response.email, role: response.role, companyId: response.companyId }
        dispatch({ type: 'LOGIN_SUCCESS', payload: { user } })
      })
      .catch(() => {
        dispatch({ type: 'LOADED' })
      })
  }, [])

  const login = async (data: LoginRequest) => {
    const response = await authApi.login(data)
    const user: User = { email: response.email, role: response.role, companyId: response.companyId }
    dispatch({ type: 'LOGIN_SUCCESS', payload: { user } })
  }

  const logout = async () => {
    try {
      await authApi.logout()
    } finally {
      // Clear local state even if the network call failed — the user shouldn't appear stuck
      // "logged in" client-side just because the revoke request didn't reach the server.
      dispatch({ type: 'LOGOUT' })
    }
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

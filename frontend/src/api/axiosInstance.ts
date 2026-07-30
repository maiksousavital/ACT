import axios from 'axios'
import { getCsrfToken } from '../utils/csrf'

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
  // Send the httpOnly auth cookie (and receive it on login) on cross-origin requests — the
  // frontend dev server and API run on different ports. No Authorization header to attach
  // client-side anymore: the browser sends the cookie automatically.
  withCredentials: true,
})

const MUTATING_METHODS = new Set(['post', 'put', 'patch', 'delete'])

axiosInstance.interceptors.request.use((config) => {
  if (config.method && MUTATING_METHODS.has(config.method)) {
    const csrfToken = getCsrfToken()
    if (csrfToken) {
      config.headers['X-XSRF-TOKEN'] = csrfToken
    }
  }
  return config
})

// Endpoints where a 401 is an expected, normal response — not a "your session died" event:
// /auth/me is the session-bootstrap check the app makes on every load, which 401s for anyone
// who isn't logged in yet, and /auth/login's 401 is just "wrong password" (LoginPage already
// shows that inline). Redirecting on either would force a hard reload of the login page itself,
// which re-triggers the same /me bootstrap check — an infinite reload loop.
const SKIP_REDIRECT_PATHS = ['/auth/me', '/auth/login']

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    const url: string = error.config?.url ?? ''
    const shouldSkip = SKIP_REDIRECT_PATHS.some((path) => url.includes(path))
    if (error.response?.status === 401 && !shouldSkip && window.location.pathname !== '/login') {
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default axiosInstance


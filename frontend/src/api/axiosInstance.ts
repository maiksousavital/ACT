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

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default axiosInstance


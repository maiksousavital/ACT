export interface LoginRequest {
  email: string
  password: string
}

export interface AuthResponse {
  token: string
  email: string
  role: string
  companyId: number | null
}

export interface User {
  email: string
  role: string
  companyId: number | null
}


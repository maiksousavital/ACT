export interface LoginRequest {
  email: string
  password: string
}

export interface AuthResponse {
  email: string
  role: string
  companyId: number | null
}

export interface User {
  email: string
  role: string
  companyId: number | null
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  token: string
  newPassword: string
}


import axiosInstance from './axiosInstance'
import type { AuthResponse, LoginRequest } from '../types/auth'

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await axiosInstance.post<AuthResponse>('/auth/login', data)
    return response.data
  },
  logout: async (): Promise<void> => {
    await axiosInstance.post('/auth/logout')
  },
  me: async (): Promise<AuthResponse> => {
    const response = await axiosInstance.get<AuthResponse>('/auth/me')
    return response.data
  },
  forgotPassword: async (email: string): Promise<{ message: string }> => {
    const response = await axiosInstance.post('/auth/forgot-password', { email })
    return response.data
  },
  resetPassword: async (token: string, newPassword: string): Promise<{ message: string }> => {
    const response = await axiosInstance.post('/auth/reset-password', { token, newPassword })
    return response.data
  },
}


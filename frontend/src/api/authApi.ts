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
}


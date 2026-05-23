import axiosInstance from './axiosInstance'
import type { AuthResponse, LoginRequest } from '../types/auth'

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await axiosInstance.post<AuthResponse>('/auth/login', data)
    return response.data
  },
}


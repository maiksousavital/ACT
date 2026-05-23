import axiosInstance from './axiosInstance'
import type { UserDto, CreateUserRequest } from '../types/user'

export const userApi = {
  getByCompany: async (companyId?: number): Promise<UserDto[]> => {
    const response = await axiosInstance.get<UserDto[]>('/user', {
      params: companyId ? { companyId } : undefined,
    })
    return response.data
  },

  getById: async (id: number): Promise<UserDto> => {
    const response = await axiosInstance.get<UserDto>(`/user/${id}`)
    return response.data
  },

  create: async (data: CreateUserRequest): Promise<UserDto> => {
    const response = await axiosInstance.post<UserDto>('/user', data)
    return response.data
  },

  update: async (id: number, data: { email?: string; companyId?: number; role?: string; isActive?: boolean }): Promise<UserDto> => {
    const response = await axiosInstance.put<UserDto>(`/user/${id}`, data)
    return response.data
  },

  delete: async (id: number): Promise<void> => {
    await axiosInstance.delete(`/user/${id}`)
  },
}


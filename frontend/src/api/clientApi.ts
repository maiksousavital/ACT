import axiosInstance from './axiosInstance'
import type { PagedResult } from '../types/common'
import type { ClientDto, CreateClientRequest, UpdateClientRequest } from '../types/client'

export const clientApi = {
  getPaged: async (page = 1, pageSize = 10): Promise<PagedResult<ClientDto>> => {
    const response = await axiosInstance.get<PagedResult<ClientDto>>('/client/paged', {
      params: { page, pageSize },
    })
    return response.data
  },

  getById: async (id: number): Promise<ClientDto> => {
    const response = await axiosInstance.get<ClientDto>(`/client/${id}`)
    return response.data
  },

  create: async (data: CreateClientRequest): Promise<ClientDto> => {
    const response = await axiosInstance.post<ClientDto>('/client', data)
    return response.data
  },

  update: async (id: number, data: UpdateClientRequest): Promise<ClientDto> => {
    const response = await axiosInstance.put<ClientDto>(`/client/${id}`, data)
    return response.data
  },
}


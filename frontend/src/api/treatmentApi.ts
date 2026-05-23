import axiosInstance from './axiosInstance'
import type { PagedResult } from '../types/common'
import type { TreatmentDto, CreateTreatmentRequest, UpdateTreatmentRequest, CompleteFollowUpRequest } from '../types/treatment'

export const treatmentApi = {
  getPaged: async (page = 1, pageSize = 10): Promise<PagedResult<TreatmentDto>> => {
    const response = await axiosInstance.get<PagedResult<TreatmentDto>>('/treatment/paged', {
      params: { page, pageSize },
    })
    return response.data
  },

  getById: async (id: number): Promise<TreatmentDto> => {
    const response = await axiosInstance.get<TreatmentDto>(`/treatment/${id}`)
    return response.data
  },

  create: async (data: CreateTreatmentRequest): Promise<TreatmentDto> => {
    const response = await axiosInstance.post<TreatmentDto>('/treatment', data)
    return response.data
  },

  update: async (id: number, data: UpdateTreatmentRequest): Promise<TreatmentDto> => {
    const response = await axiosInstance.put<TreatmentDto>(`/treatment/${id}`, data)
    return response.data
  },

  getByClient: async (clientId: number): Promise<TreatmentDto[]> => {
    const response = await axiosInstance.get<TreatmentDto[]>(`/treatment/by-client/${clientId}`)
    return response.data
  },
}

export const followUpApi = {
  getToday: async (): Promise<TreatmentDto[]> => {
    const response = await axiosInstance.get<TreatmentDto[]>('/followups/today')
    return response.data
  },

  getDue: async (): Promise<TreatmentDto[]> => {
    const response = await axiosInstance.get<TreatmentDto[]>('/followups/due')
    return response.data
  },

  complete: async (id: number, data: CompleteFollowUpRequest): Promise<TreatmentDto> => {
    const response = await axiosInstance.post<TreatmentDto>(`/followups/${id}/complete`, data)
    return response.data
  },
}


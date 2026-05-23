import axiosInstance from './axiosInstance'
import type { PagedResult } from '../types/common'
import type {
  TreatmentTypeDto,
  CreateTreatmentTypeRequest,
  UpdateTreatmentTypeRequest,
  TreatmentTypeMetadata,
} from '../types/treatmentType'

export const treatmentTypeApi = {
  getPaged: async (page = 1, pageSize = 10): Promise<PagedResult<TreatmentTypeDto>> => {
    const response = await axiosInstance.get<PagedResult<TreatmentTypeDto>>('/treatmenttype/paged', {
      params: { page, pageSize },
    })
    return response.data
  },

  getById: async (id: number): Promise<TreatmentTypeDto> => {
    const response = await axiosInstance.get<TreatmentTypeDto>(`/treatmenttype/${id}`)
    return response.data
  },

  getMetadata: async (): Promise<TreatmentTypeMetadata> => {
    const response = await axiosInstance.get<TreatmentTypeMetadata>('/treatmenttype/add-edit-metadata')
    return response.data
  },

  create: async (data: CreateTreatmentTypeRequest): Promise<TreatmentTypeDto> => {
    const response = await axiosInstance.post<TreatmentTypeDto>('/treatmenttype', data)
    return response.data
  },

  update: async (id: number, data: UpdateTreatmentTypeRequest): Promise<TreatmentTypeDto> => {
    const response = await axiosInstance.put<TreatmentTypeDto>(`/treatmenttype/${id}`, data)
    return response.data
  },
}


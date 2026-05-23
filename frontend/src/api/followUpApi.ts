import axiosInstance from './axiosInstance'
import type { TreatmentDto } from '../types/treatment'

export const followUpApi = {
  getToday: async (): Promise<TreatmentDto[]> => {
    const response = await axiosInstance.get<TreatmentDto[]>('/followups/today')
    return response.data
  },
  getDue: async (): Promise<TreatmentDto[]> => {
    const response = await axiosInstance.get<TreatmentDto[]>('/followups/due')
    return response.data
  },
}


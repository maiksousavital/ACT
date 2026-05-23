import axiosInstance from './axiosInstance'
import type { PagedResult } from '../types/common'
import type { CompanyDto, CreateCompanyRequest, UpdateCompanyRequest } from '../types/company'

export const companyApi = {
  getPaged: async (page = 1, pageSize = 10): Promise<PagedResult<CompanyDto>> => {
    const response = await axiosInstance.get<PagedResult<CompanyDto>>('/company/paged', {
      params: { page, pageSize },
    })
    return response.data
  },

  getById: async (id: number): Promise<CompanyDto> => {
    const response = await axiosInstance.get<CompanyDto>(`/company/${id}`)
    return response.data
  },

  create: async (data: CreateCompanyRequest): Promise<CompanyDto> => {
    const response = await axiosInstance.post<CompanyDto>('/company', data)
    return response.data
  },

  update: async (id: number, data: UpdateCompanyRequest): Promise<CompanyDto> => {
    const response = await axiosInstance.put<CompanyDto>(`/company/${id}`, data)
    return response.data
  },

  delete: async (id: number): Promise<void> => {
    await axiosInstance.delete(`/company/${id}`)
  },
}


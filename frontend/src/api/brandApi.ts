import axiosInstance from './axiosInstance'
import type { BrandSettingsDto, CreateBrandSettingsRequest, UpdateBrandSettingsRequest } from '../types/brand'

export const brandApi = {
  get: async (companyId?: number): Promise<BrandSettingsDto | null> => {
    try {
      const response = await axiosInstance.get<BrandSettingsDto>('/brandsettings', {
        params: companyId ? { companyId } : undefined,
      })
      return response.data
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { status?: number } }
        if (axiosErr.response?.status === 404) return null
      }
      throw err
    }
  },

  create: async (data: CreateBrandSettingsRequest, companyId?: number): Promise<BrandSettingsDto> => {
    const response = await axiosInstance.post<BrandSettingsDto>('/brandsettings', data, {
      params: companyId ? { companyId } : undefined,
    })
    return response.data
  },

  update: async (data: UpdateBrandSettingsRequest, companyId?: number): Promise<BrandSettingsDto> => {
    const response = await axiosInstance.put<BrandSettingsDto>('/brandsettings', data, {
      params: companyId ? { companyId } : undefined,
    })
    return response.data
  },
}


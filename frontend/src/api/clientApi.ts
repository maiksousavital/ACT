import axiosInstance from './axiosInstance'
import type { PagedResult } from '../types/common'
import type { ClientDto } from '../types/client'

export const clientApi = {
  getPaged: async (page = 1, pageSize = 10): Promise<PagedResult<ClientDto>> => {
    const response = await axiosInstance.get<PagedResult<ClientDto>>('/client/paged', {
      params: { page, pageSize },
    })
    return response.data
  },
}


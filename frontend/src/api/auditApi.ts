import axiosInstance from './axiosInstance'
import type { PagedResult } from '../types/common'
import type { AuditLogDto, LoginHistoryDto } from '../types/audit'

export const auditApi = {
  getLogs: async (page = 1, pageSize = 50, companyId?: number): Promise<PagedResult<AuditLogDto>> => {
    const response = await axiosInstance.get<PagedResult<AuditLogDto>>('/audit/logs', {
      params: { page, pageSize, ...(companyId ? { companyId } : {}) },
    })
    return response.data
  },

  getLoginHistory: async (page = 1, pageSize = 50, companyId?: number): Promise<PagedResult<LoginHistoryDto>> => {
    const response = await axiosInstance.get<PagedResult<LoginHistoryDto>>('/audit/logins', {
      params: { page, pageSize, ...(companyId ? { companyId } : {}) },
    })
    return response.data
  },

  getForEntity: async (
    entityType: string,
    entityId: number,
    params: { page?: number; pageSize?: number; search?: string; from?: string; to?: string } = {},
  ): Promise<PagedResult<AuditLogDto>> => {
    const { page = 1, pageSize = 20, search, from, to } = params
    const response = await axiosInstance.get<PagedResult<AuditLogDto>>(`/audit/entity/${entityType}/${entityId}`, {
      params: {
        page,
        pageSize,
        ...(search ? { search } : {}),
        ...(from ? { from } : {}),
        ...(to ? { to } : {}),
      },
    })
    return response.data
  },
}


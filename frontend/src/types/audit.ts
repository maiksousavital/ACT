export interface AuditLogDto {
  id: number
  userId?: number
  userEmail: string
  companyId?: number
  action: string
  entityType: string
  entityId?: number
  details?: string
  timestamp: string
}

export interface LoginHistoryDto {
  id: number
  userId?: number
  email: string
  companyId?: number
  ipAddress?: string
  userAgent?: string
  success: boolean
  failureReason?: string
  timestamp: string
}


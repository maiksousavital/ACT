export interface ClientDto {
  id: number
  firstName: string
  lastName: string
  email?: string
  phone?: string
  notes?: string
  isDeleted: boolean
  createdAt: string
  companyId?: number
}

export interface CreateClientRequest {
  companyId?: number
  firstName: string
  lastName: string
  email?: string
  phone?: string
  notes?: string
}

export interface UpdateClientRequest {
  firstName: string
  lastName: string
  email?: string
  phone?: string
  notes?: string
}


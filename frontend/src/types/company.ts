export interface CompanyDto {
  id: number
  name: string
  contactEmail?: string
  phone?: string
  address?: string
}

export interface CreateCompanyRequest {
  name: string
  contactEmail?: string
  phone?: string
  address?: string
}

export interface UpdateCompanyRequest {
  name: string
  contactEmail?: string
  phone?: string
  address?: string
}


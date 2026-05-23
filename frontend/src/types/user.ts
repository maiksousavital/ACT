export interface UserDto {
  id: number
  email: string
  companyId?: number
  role: string
  isActive: boolean
  createdAt: string
}

export interface CreateUserRequest {
  email: string
  password: string
  companyId: number
  role: 'Admin' | 'User'
}


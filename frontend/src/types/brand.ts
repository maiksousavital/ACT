export interface BrandSettingsDto {
  id: number
  companyId: number
  primaryColor?: string
  secondaryColor?: string
  accentColor?: string
  theme?: string
  logoUrl?: string
}

export interface UpdateBrandSettingsRequest {
  primaryColor?: string
  secondaryColor?: string
  accentColor?: string
  theme?: string
  logoUrl?: string
}

export type CreateBrandSettingsRequest = UpdateBrandSettingsRequest


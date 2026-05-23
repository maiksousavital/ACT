export interface TreatmentTypeDto {
  id: number
  name: string
  followUpIntervalDays: number
  notes?: string
}

export interface CreateTreatmentTypeRequest {
  name: string
  followUpIntervalDays: number
  isActive?: boolean
}

export interface UpdateTreatmentTypeRequest {
  name: string
  followUpIntervalDays: number
  isActive?: boolean
}

export interface FollowUpPeriodOption {
  name: string
  days: number
}

export interface TreatmentTypeMetadata {
  allowedFollowUpPeriods: FollowUpPeriodOption[]
}


export interface TreatmentDto {
  id: number
  clientId: number
  clientName?: string
  treatmentTypeId: number
  treatmentTypeName?: string
  phone?: string
  treatmentDate: string
  nextFollowUpDate: string
  notes?: string
  followedUpAt?: string
  followUpNotes?: string
  isFollowedUp: boolean
  isDue: boolean
}

export interface CreateTreatmentRequest {
  clientId: number
  treatmentTypeId: number
  treatmentDate: string
  notes?: string
}

export interface UpdateTreatmentRequest {
  treatmentDate: string
  notes?: string
}

export interface CompleteFollowUpRequest {
  followUpNotes?: string
}


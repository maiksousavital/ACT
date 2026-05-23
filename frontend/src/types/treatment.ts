export interface TreatmentDto {
  id: number
  clientId: number
  clientFirstName?: string
  clientLastName?: string
  treatmentTypeId: number
  treatmentTypeName?: string
  treatmentDate: string
  nextFollowUpDate: string
  notes?: string
  followedUpAt?: string
  followUpNotes?: string
  isFollowedUp: boolean
  isDue: boolean
}


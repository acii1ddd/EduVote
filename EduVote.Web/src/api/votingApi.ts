import api from './axiosClient'

export type VotingStatus = 'Draft' | 'Active' | 'Paused' | 'Finished' | 'PendingApproval'
export type VotingType = 'SingleChoice' | 'MultipleChoice' | 'Rating' | 'OpenAnswer'

export interface VotingResponse {
    id: string
    title: string
    description: string
    type: VotingType
    isAnonymous: boolean
    allowVoteChange: boolean
    startTime: string
    endTime: string
    status: VotingStatus
    createdAt: string
}

export interface GetVotingsResponse {
    votings: VotingResponse[]
}

export interface VotingFormPayload {
    title: string
    description: string
    type: VotingType
    isAnonymous: boolean
    allowVoteChange: boolean
    startTime: string
    endTime: string
}

export const getVotingsForUser = async (userId: string): Promise<GetVotingsResponse> => {
    const response = await api.get<GetVotingsResponse>(`/users/${userId}/votings`)
    return response.data
}

export const getVotings = async (): Promise<GetVotingsResponse> => {
    const response = await api.get<GetVotingsResponse>('/votings')
    return response.data
}

export const createVoting = async (payload: VotingFormPayload): Promise<VotingResponse> => {
    const response = await api.post<VotingResponse>('/votings', payload)
    return response.data
}

export const updateVoting = async (id: string, payload: VotingFormPayload): Promise<VotingResponse> => {
    const response = await api.put<VotingResponse>(`/votings/${id}`, payload)
    return response.data
}

export const deleteVoting = async (id: string): Promise<void> => {
    await api.delete(`/votings/${id}`)
}

export const startVoting = async (id: string): Promise<void> => {
    await api.post(`/votings/${id}/start`)
}

export const pauseVoting = async (id: string): Promise<void> => {
    await api.post(`/votings/${id}/pause`)
}

export const finishVoting = async (id: string): Promise<void> => {
    await api.post(`/votings/${id}/finish`)
}

export const approveVoting = async (id: string): Promise<void> => {
    await api.post(`/votings/${id}/approve`)
}

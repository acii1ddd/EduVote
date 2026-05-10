import api from './axiosClient'

export type VotingStatus = 'Draft' | 'Active' | 'Paused' | 'Finished'
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

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

export const getVotingsForUser = async (userId: string): Promise<GetVotingsResponse> => {
    const response = await api.get<GetVotingsResponse>(`/users/${userId}/votings`)
    return response.data
}

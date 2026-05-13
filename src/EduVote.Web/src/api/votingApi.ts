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
    createdById: string
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

export const getVotingsCreatedByUser = async (): Promise<GetVotingsResponse> => {
    const response = await api.get<GetVotingsResponse>('/votings/created')
    return response.data
}

export const getVotings = async (): Promise<GetVotingsResponse> => {
    const response = await api.get<GetVotingsResponse>('/votings')
    return response.data
}

export const getVotingById = async (id: string): Promise<VotingResponse> => {
    const response = await api.get<VotingResponse>(`/votings/${id}`)
    return response.data
}

const STATUS_ORDER: Record<VotingStatus, number> = {
    Active:          0,
    PendingApproval: 1,
    Draft:           2,
    Paused:          3,
    Finished:        4,
}

export const sortVotingsFinishedLast = (list: VotingResponse[]): VotingResponse[] =>
    [...list].sort((a, b) => STATUS_ORDER[a.status] - STATUS_ORDER[b.status])

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

export interface CastVotePayload {
    selectedCandidateId?: string
    selectedCandidateIds?: string[]
    ratingAnswers?: Record<string, number>
    textAnswer?: string
}

export interface CastVoteResult {
    voteId: string
    voteHash: string
    createdAt: string
}

export const castVote = async (votingId: string, payload: CastVotePayload): Promise<CastVoteResult> => {
    const response = await api.post<CastVoteResult>(`/votings/${votingId}/vote`, payload)
    return response.data
}

export const getVotedVotingIds = async (): Promise<Set<string>> => {
    const response = await api.get<{ votingIds: string[] }>('/votings/voted', {
        headers: { 'Cache-Control': 'no-cache' },
    })
    return new Set(response.data.votingIds ?? [])
}

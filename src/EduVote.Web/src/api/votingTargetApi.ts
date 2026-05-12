import api from './axiosClient'

export interface VotingTargetResponse {
    votingId: string
    educationUnitId: string
}

export interface GetVotingTargetsResponse {
    targets: VotingTargetResponse[]
}

export const getTargets = async (votingId: string): Promise<GetVotingTargetsResponse> => {
    const response = await api.get<GetVotingTargetsResponse>(`/votings/${votingId}/targets`)
    return response.data
}

export const addTarget = async (votingId: string, educationUnitId: string): Promise<VotingTargetResponse> => {
    const response = await api.post<VotingTargetResponse>(`/votings/${votingId}/targets`, {
        votingId,
        educationUnitId,
    })
    return response.data
}

export const deleteTarget = async (votingId: string, educationUnitId: string): Promise<void> => {
    await api.delete(`/votings/${votingId}/targets/${educationUnitId}`)
}

import api from './axiosClient'

export interface CandidateResponse {
    id: string
    votingId: string
    name: string
    description: string
    photoUrl: string
}

export interface GetCandidatesResponse {
    candidates: CandidateResponse[]
}

export const getCandidates = async (votingId: string): Promise<GetCandidatesResponse> => {
    const response = await api.get<GetCandidatesResponse>(`/votings/${votingId}/candidates`)
    return response.data
}

export const createCandidate = async (
    votingId: string,
    name: string,
    description: string,
): Promise<CandidateResponse> => {
    const response = await api.post<CandidateResponse>(`/votings/${votingId}/candidates`, {
        votingId,
        name,
        description,
    })
    return response.data
}

export const deleteCandidate = async (votingId: string, candidateId: string): Promise<void> => {
    await api.delete(`/votings/${votingId}/candidates/${candidateId}`)
}

export const uploadCandidatePhoto = async (
    candidateId: string,
    photo: File,
): Promise<{ photo_url: string }> => {
    const formData = new FormData()
    formData.append('photo', photo)
    const response = await api.post<{ photo_url: string }>(
        `/candidates/${candidateId}/photo`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } },
    )
    return response.data
}

export const deleteCandidatePhoto = async (candidateId: string): Promise<void> => {
    await api.delete(`/candidates/${candidateId}/photo`)
}

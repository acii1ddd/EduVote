import api from "@/api/axiosClient.ts";

export interface EducationUnit {
    id: string
    name: string
    type: string
    parentId: string
}

export interface GetEducationUnitsResponse {
    educationUnits: EducationUnit[]
}

export const getEducationUnits = async (
    
) => {
    const response = await api.get<GetEducationUnitsResponse>(
        '/education-units'
    )

    return response.data
}
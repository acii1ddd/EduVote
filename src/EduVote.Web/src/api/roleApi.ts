import api from "@/api/axiosClient.ts";

export interface RoleResponse {
    name: string
}

export interface GetRolesResponse {
    roles: RoleResponse[]
}

export const getRoles = async (
    
) => {
    const res = await api.get<GetRolesResponse>("/roles")
    return res.data
}
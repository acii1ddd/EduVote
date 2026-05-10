import api from "@/api/axiosClient.ts";

export interface UserResponse {
    id: string
    email: string
    role: string,
    educationUnitId: string,
    educationUnitName: string,
    name: string,
    createdAt: string
}

export interface GetUsersResponse {
    users: UserResponse[]
}

export const getUsers = async () => {
    const response = await api
        .get<GetUsersResponse>(
        '/users'
    )

    return response.data
}

export interface UpdateUserRequest {
    id: string
    name: string
    email: string
    role: string
}

export const updateUser = async (
    data: UpdateUserRequest
) => {

    await api.put(
        `/users/${data.id}`,
        data
    )
}

export const deleteUserRequest = async (
    userId: string
) => {

    await api.delete(
        `/users/${userId}`
    )
}
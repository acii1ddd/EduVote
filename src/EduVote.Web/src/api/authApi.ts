import api from "./axiosClient.ts";

export interface RegisterRequest {
    email: string
    password: string
    name: string
}

export interface LoginRequest {
    email: string
    password: string
}

export interface RegisterResponse {
    userId: string
}

export interface LoginResponse {
    userId: string,
    role: string,
    accessToken: string
}

export const register = async (
    data: RegisterRequest
) => {
    const response = await api
        .post<RegisterResponse>('/auth/register', data)
    
    return response.data
}

export const login = async (
    data: LoginRequest) => {
    const response = await api
        .post<LoginResponse>('/auth/login', data)

    return response.data
}
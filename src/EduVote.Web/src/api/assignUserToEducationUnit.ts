import api from "@/api/axiosClient.ts";

export const assignUserToEducationUnit = async (
    userId: string,
    educationUnitId: string
) => {

    await api.post(
        `/users/${userId}/education-units/${educationUnitId}`
    )
}
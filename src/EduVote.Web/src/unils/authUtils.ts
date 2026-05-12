import { jwtDecode } from "jwt-decode";

export type JwtPayload = {
    nameid: string,
    role: string;
};

export const getClaimsFromToken = (
    token: string
): JwtPayload => {
    return jwtDecode<JwtPayload>(token)
}
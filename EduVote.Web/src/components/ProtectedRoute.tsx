import {getClaimsFromToken} from "@/unils/authUtils.ts";
import { Navigate } from "react-router-dom"

type Props = {
    children: React.ReactNode
    allowedRoles: string[]
}

export default function ProtectedRoute({
                                           children,
                                           allowedRoles
                                       }: Props
) {

    const token = localStorage.getItem('access_token')

    if (!token) {
        return <Navigate to="/login" replace />
    }

    const claims = getClaimsFromToken(token)

    if (!claims?.role) {
        return <Navigate to="/login" replace />
    }

    if (!allowedRoles.includes(claims.role)) {
        return <Navigate to="/forbidden" replace />
    }

    return children
}
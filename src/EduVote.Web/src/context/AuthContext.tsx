import { createContext, useContext, useState } from 'react'
import { getClaimsFromToken, type JwtPayload } from '@/unils/authUtils'

type AuthContextValue = {
    token: string | null
    claims: JwtPayload | null
    setToken: (token: string | null) => void
    logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [token, setTokenState] = useState<string | null>(
        () => localStorage.getItem('access_token')
    )

    let claims: JwtPayload | null = null
    if (token) {
        try {
            claims = getClaimsFromToken(token)
        } catch {
            claims = null
        }
    }

    const setToken = (newToken: string | null) => {
        if (newToken) {
            localStorage.setItem('access_token', newToken)
        } else {
            localStorage.removeItem('access_token')
        }
        setTokenState(newToken)
    }

    return (
        <AuthContext.Provider value={{ token, claims, setToken, logout: () => setToken(null) }}>
            {children}
        </AuthContext.Provider>
    )
}

export function useAuth() {
    const ctx = useContext(AuthContext)
    if (!ctx) throw new Error('useAuth must be used within AuthProvider')
    return ctx
}

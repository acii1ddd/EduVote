import { useEffect, useState } from 'react'
import { parseJwt } from '../utils/jwt'

export default function Home() {
  const [role, setRole] = useState<string | null>(null)
  const [userId, setUserId] = useState<string | null>(null)

  useEffect(() => {
    const token = localStorage.getItem('access_token')
    const claims: any = parseJwt(token || undefined)
    if (claims) {
      setRole(claims.role || null)
      setUserId(claims.nameid || null)
    }
  }, [])

  return (
    <div>
      <h1>Home</h1>
      {role ? (
        <p>Hello, {role} {userId ? `(${userId})` : ''}</p>
      ) : (
        <p>You are not logged in.</p>
      )}
    </div>
  )
}

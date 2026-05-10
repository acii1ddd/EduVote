import { useEffect } from "react"
import { useNavigate } from "react-router-dom"
import {getClaimsFromToken} from "@/unils/authUtils.ts";

export default function Home() {

  const navigate = useNavigate()

  useEffect(() => {

    const token = localStorage.getItem('access_token')

    if (!token) {
      navigate('/login')
      return
    }

    const claims = getClaimsFromToken(token)

    if (!claims?.role) {
      navigate('/login')
      return
    }

    switch (claims.role) {

      case 'Administrator':
        navigate('/admin')
        break

      case 'Teacher':
        navigate('/dashboard')
        break

      case 'Student':
        navigate('/dashboard')
        break

      default:
        navigate('/login')
        break
    }

  }, [navigate])

  return null
}
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../api/axiosClient'
import { parseJwt } from '../utils/jwt'

export default function Login() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const navigate = useNavigate()

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      const resp = await api.post('/auth/login', { email, password })
      const data = resp.data
      if (data.access_token) {
        localStorage.setItem('access_token', data.access_token)
      }
      // optional: store role or parse it when needed
      const claims = parseJwt(data.access_token)
      // navigate to home
      navigate('/')
    } catch (err: any) {
      setError(err?.response?.data?.message || String(err))
    }
  }

  return (
    <div style={{maxWidth:480}}>
      <h2>Login</h2>
      <form onSubmit={submit}>
        <div>
          <label>Email</label>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div>
          <label>Password</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        {error && <div style={{color:'red'}}>{error}</div>}
        <button type="submit">Login</button>
      </form>
    </div>
  )
}

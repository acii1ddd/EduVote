import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../api/axiosClient'

export default function Register() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [name, setName] = useState('')
  const [error, setError] = useState<string | null>(null)
  const navigate = useNavigate()

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      await api.post('/auth/register', { email, password, name })
      navigate('/login')
    } catch (err: any) {
      setError(err?.response?.data?.message || String(err))
    }
  }

  return (
    <div style={{maxWidth:480}}>
      <h2>Register</h2>
      <form onSubmit={submit}>
        <div>
          <label>Name</label>
          <input value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div>
          <label>Email</label>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div>
          <label>Password</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        {error && <div style={{color:'red'}}>{error}</div>}
        <button type="submit">Register</button>
      </form>
    </div>
  )
}

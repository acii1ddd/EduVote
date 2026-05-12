import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { login, type LoginResponse } from '../api/authApi'
import { useAuth } from '@/context/AuthContext'
import { Vote } from 'lucide-react'

export default function Login() {
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [error, setError] = useState<string | null>(null)
    const [isLoading, setIsLoading] = useState(false)

    const navigate = useNavigate()
    const { setToken } = useAuth()

    const submit = async (e: React.FormEvent) => {
        e.preventDefault()
        setError(null)

        if (!email.trim() || !password.trim()) {
            setError('Заполните все поля')
            return
        }

        setIsLoading(true)

        try {
            const data: LoginResponse = await login({ email, password })
            setToken(data.accessToken)
            navigate('/')
        } catch (err: any) {
            setError(err?.response?.data?.message || 'Неверный email или пароль')
        } finally {
            setIsLoading(false)
        }
    }

    const inputClass = `
        block h-11 w-full rounded-xl border border-border bg-background
        px-3.5 text-sm text-foreground placeholder:text-muted-foreground
        focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20
        transition-colors
    `

    return (
        <div className="flex min-h-[calc(100vh-64px)] items-center justify-center py-12">
            <div className="w-full max-w-sm">

                {/* Logo */}
                <div className="mb-8 text-center">
                    <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-primary shadow-lg shadow-primary/25">
                        <Vote className="h-7 w-7 text-primary-foreground" />
                    </div>
                    <h1 className="text-2xl font-bold text-foreground">Вход в EduVote</h1>
                    <p className="mt-1.5 text-sm text-muted-foreground">
                        Введите данные для входа в аккаунт
                    </p>
                </div>

                {/* Card */}
                <div className="rounded-2xl border border-border bg-card px-8 py-8 shadow-sm">
                    <form onSubmit={submit} className="space-y-5">

                        <div className="space-y-1.5">
                            <label htmlFor="email" className="block text-sm font-medium text-foreground">
                                Email
                            </label>
                            <input
                                id="email"
                                type="email"
                                autoComplete="email"
                                placeholder="you@example.com"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                className={inputClass}
                            />
                        </div>

                        <div className="space-y-1.5">
                            <label htmlFor="password" className="block text-sm font-medium text-foreground">
                                Пароль
                            </label>
                            <input
                                id="password"
                                type="password"
                                autoComplete="current-password"
                                placeholder="Введите пароль"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                className={inputClass}
                            />
                        </div>

                        {error && (
                            <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-3.5 py-2.5 text-sm font-medium text-destructive">
                                {error}
                            </div>
                        )}

                        <button
                            type="submit"
                            disabled={isLoading}
                            className="flex h-11 w-full items-center justify-center rounded-xl bg-primary text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/30 transition-all hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 focus:ring-offset-card disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            {isLoading ? 'Входим...' : 'Войти'}
                        </button>

                    </form>
                </div>

                <p className="mt-6 text-center text-sm text-muted-foreground">
                    Нет аккаунта?{' '}
                    <Link to="/register" className="font-semibold text-primary hover:opacity-80 transition-opacity">
                        Зарегистрироваться
                    </Link>
                </p>

            </div>
        </div>
    )
}

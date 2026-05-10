import { useNavigate, Link } from 'react-router-dom'
import { useState } from 'react'
import { register } from '../api/authApi'
import { Vote } from 'lucide-react'

export function RegisterForm() {
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [name, setName] = useState('')
    const [error, setError] = useState<string | null>(null)
    const [isLoading, setIsLoading] = useState(false)

    const navigate = useNavigate()

    const submit = async (e: React.FormEvent) => {
        e.preventDefault()
        setError(null)

        if (!name.trim() || !email.trim() || !password.trim()) {
            setError('Заполните все поля')
            return
        }

        setIsLoading(true)

        try {
            await register({ email, password, name })
            navigate('/login')
        } catch (err: any) {
            setError(err?.response?.data?.message || String(err))
        } finally {
            setIsLoading(false)
        }
    }

    const inputClass = "block h-10 w-full rounded-lg border border-gray-300 bg-transparent px-3 text-sm text-gray-900 placeholder:text-gray-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 dark:border-gray-700 dark:text-gray-100 dark:focus:border-indigo-400"
    const labelClass = "block text-sm font-medium text-gray-700 dark:text-gray-300"

    return (
        <div className="flex min-h-[calc(100vh-64px)] items-center justify-center py-12">
            <div className="w-full max-w-sm">

                <div className="mb-8 text-center">
                    <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-600 text-white shadow-lg shadow-indigo-200 dark:shadow-indigo-900/40">
                        <Vote className="h-6 w-6" />
                    </div>
                    <h1 className="text-2xl font-bold tracking-tight text-gray-900 dark:text-white">
                        Создать аккаунт
                    </h1>
                    <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                        Заполните данные для регистрации
                    </p>
                </div>

                <form
                    onSubmit={submit}
                    className="rounded-2xl border border-gray-200 bg-white px-8 py-8 shadow-sm dark:border-gray-800 dark:bg-gray-900"
                >
                    <div className="space-y-5">

                        <div className="space-y-1.5">
                            <label htmlFor="name" className={labelClass}>Имя</label>
                            <input
                                id="name"
                                type="text"
                                autoComplete="name"
                                placeholder="Иван Иванов"
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                                className={inputClass}
                            />
                        </div>

                        <div className="space-y-1.5">
                            <label htmlFor="email" className={labelClass}>Email</label>
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
                            <label htmlFor="password" className={labelClass}>Пароль</label>
                            <input
                                id="password"
                                type="password"
                                autoComplete="new-password"
                                placeholder="Минимум 8 символов"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                className={inputClass}
                            />
                        </div>

                        {error && (
                            <div className="rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-600 dark:bg-red-900/20 dark:text-red-400">
                                {error}
                            </div>
                        )}

                        <button
                            type="submit"
                            disabled={isLoading}
                            className="flex h-10 w-full items-center justify-center rounded-lg bg-indigo-600 text-sm font-semibold text-white transition-colors hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-indigo-500 dark:hover:bg-indigo-400"
                        >
                            {isLoading ? 'Создаём...' : 'Создать аккаунт'}
                        </button>

                    </div>
                </form>

                <p className="mt-6 text-center text-sm text-gray-500 dark:text-gray-400">
                    Уже есть аккаунт?{' '}
                    <Link to="/login" className="font-medium text-indigo-600 hover:text-indigo-500 dark:text-indigo-400">
                        Войти
                    </Link>
                </p>

            </div>
        </div>
    )
}

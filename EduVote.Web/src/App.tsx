import './App.css'

import { Routes, Route, NavLink, useNavigate } from 'react-router-dom'
import { LogOut, Vote } from 'lucide-react'

import Home from './pages/Home'
import Register from './pages/Register'
import Login from './pages/Login'
import AdminPage from './pages/AdminPage'
import ProtectedRoute from './components/ProtectedRoute'
import { useAuth } from './context/AuthContext'

function Navbar() {
    const { claims, logout } = useAuth()
    const navigate = useNavigate()

    const navLink = ({ isActive }: { isActive: boolean }) =>
        `px-3 py-1.5 rounded-md text-sm font-medium transition-colors duration-150
        ${isActive
            ? 'bg-indigo-50 text-indigo-700 dark:bg-indigo-900/40 dark:text-indigo-300'
            : 'text-gray-500 hover:text-gray-900 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-white dark:hover:bg-gray-800'
        }`

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    return (
        <header className="sticky top-0 z-10 border-b border-gray-200 bg-white/90 backdrop-blur-sm dark:border-gray-800 dark:bg-gray-900/90">
            <nav className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3 sm:px-6">

                <NavLink to="/" className="flex items-center gap-2 text-indigo-600 dark:text-indigo-400">
                    <Vote className="h-5 w-5" />
                    <span className="text-base font-bold tracking-tight">EduVote</span>
                </NavLink>

                <div className="flex items-center gap-1">
                    {claims ? (
                        <>
                            <span className="mr-2 rounded-full bg-indigo-100 px-3 py-1 text-xs font-semibold text-indigo-700 dark:bg-indigo-900/50 dark:text-indigo-300">
                                {claims.role}
                            </span>
                            <button
                                onClick={handleLogout}
                                className="flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium text-gray-500 transition-colors hover:bg-red-50 hover:text-red-600 dark:text-gray-400 dark:hover:bg-red-900/20 dark:hover:text-red-400"
                            >
                                <LogOut className="h-4 w-4" />
                                Выйти
                            </button>
                        </>
                    ) : (
                        <>
                            <NavLink to="/register" className={navLink}>
                                Регистрация
                            </NavLink>
                            <NavLink to="/login" className={navLink}>
                                Войти
                            </NavLink>
                        </>
                    )}
                </div>

            </nav>
        </header>
    )
}

function App() {
    return (
        <div className="min-h-screen bg-gray-50 text-gray-900 dark:bg-gray-950 dark:text-gray-100">

            <Navbar />

            <main className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/login" element={<Login />} />
                    <Route
                        path="/admin"
                        element={
                            <ProtectedRoute allowedRoles={['Administrator']}>
                                <AdminPage />
                            </ProtectedRoute>
                        }
                    />
                </Routes>
            </main>

        </div>
    )
}

export default App

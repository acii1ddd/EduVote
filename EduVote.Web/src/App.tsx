import './App.css'

import { Routes, Route, NavLink, useNavigate } from 'react-router-dom'
import { LogOut, Vote } from 'lucide-react'

import Home from './pages/Home'
import Register from './pages/Register'
import Login from './pages/Login'
import AdminPage from './pages/AdminPage'
import StudentDashboard from './pages/StudentDashboard'
import TeacherDashboard from './pages/TeacherDashboard'
import VotingManagement from './pages/VotingManagement'
import ProtectedRoute from './components/ProtectedRoute'
import { useAuth } from './context/AuthContext'

function Navbar() {
    const { claims, logout } = useAuth()
    const navigate = useNavigate()

    const navLinkClass = ({ isActive }: { isActive: boolean }) =>
        `px-3 py-1.5 rounded-lg text-sm font-medium transition-colors duration-150
        ${isActive
            ? 'bg-primary/10 text-primary'
            : 'text-muted-foreground hover:text-foreground hover:bg-secondary'
        }`

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    return (
        <header className="sticky top-0 z-10 border-b border-border bg-card/90 backdrop-blur-sm">
            <nav className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3 sm:px-6">

                <NavLink to="/" className="flex items-center gap-2 text-primary font-bold">
                    <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                        <Vote className="h-4 w-4" />
                    </div>
                    <span className="text-base tracking-tight">EduVote</span>
                </NavLink>

                <div className="flex items-center gap-1">
                    {claims ? (
                        <>
                            {claims.role === 'Student' && (
                                <NavLink to="/dashboard" className={navLinkClass}>
                                    Дашборд
                                </NavLink>
                            )}
                            {claims.role === 'Teacher' && (
                                <NavLink to="/teacher-dashboard" className={navLinkClass}>
                                    Дашборд
                                </NavLink>
                            )}
                            {claims.role === 'Administrator' && (
                                <NavLink to="/votings" className={navLinkClass}>
                                    Голосования
                                </NavLink>
                            )}
                            {claims.role === 'Administrator' && (
                                <NavLink to="/admin" className={navLinkClass}>
                                    Администрирование
                                </NavLink>
                            )}
                            <span className="mx-2 rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
                                {claims.role}
                            </span>
                            <button
                                onClick={handleLogout}
                                className="flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive"
                            >
                                <LogOut className="h-4 w-4" />
                                Выйти
                            </button>
                        </>
                    ) : (
                        <>
                            <NavLink to="/register" className={navLinkClass}>
                                Регистрация
                            </NavLink>
                            <NavLink to="/login" className={navLinkClass}>
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
        <div className="min-h-screen bg-background text-foreground">

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
                    <Route
                        path="/dashboard"
                        element={
                            <ProtectedRoute allowedRoles={['Student']}>
                                <StudentDashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/teacher-dashboard"
                        element={
                            <ProtectedRoute allowedRoles={['Teacher']}>
                                <TeacherDashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/votings"
                        element={
                            <ProtectedRoute allowedRoles={['Administrator']}>
                                <VotingManagement />
                            </ProtectedRoute>
                        }
                    />
                </Routes>
            </main>

        </div>
    )
}

export default App
